using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace QRZNetwork
{
    /// <summary>
    /// TCP client with a VB6 Winsock-like surface (Connect / Send / Close, State,
    /// Connected / DataArrival / SendComplete / Disconnected / Error events),
    /// implemented on top of <see cref="TcpClient"/> and async/await.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Events are raised on the <see cref="SynchronizationContext"/> captured when the
    /// object is created. Create it on the UI thread (e.g. inside a Form) and handlers can
    /// touch controls directly, with no Invoke needed. Without a context, events are raised
    /// on thread-pool threads.
    /// </para>
    /// <para>
    /// Sends are queued and written in order by a single writer. <see cref="SendInterval"/>
    /// optionally spaces them out, which replaces "Send + Thread.Sleep" sequences.
    /// </para>
    /// <para>
    /// Unlike VB6, <see cref="Disconnected"/> is raised whenever an established connection
    /// ends: closed by the remote host, closed locally with <see cref="Close"/>, or lost
    /// because of an error (in that case Disconnected comes first, then ErrorReceived).
    /// </para>
    /// </remarks>
    public sealed class WinsockClient : IDisposable
    {
        private const int MaxLineLength = 64 * 1024;

        private readonly SynchronizationContext? _context;
        private readonly object _sync = new object();
        private Session? _session;
        private WinsockState _state = WinsockState.Closed;
        private bool _disposed;

        /// <summary>Creates the client bound to the current SynchronizationContext (the UI thread when called from a Form).</summary>
        public WinsockClient() : this(SynchronizationContext.Current)
        {
        }

        /// <summary>Creates the client bound to an explicit context (null = raise events on thread-pool threads).</summary>
        public WinsockClient(SynchronizationContext? context)
        {
            _context = context;
        }

        #region Events

        /// <summary>The connection has been established (VB6: Connect).</summary>
        public event EventHandler? Connected;

        /// <summary>An established connection has ended (VB6: Close, but also raised on local Close and on errors).</summary>
        public event EventHandler? Disconnected;

        /// <summary>A block of data has arrived (VB6: DataArrival). Data is already read: no GetData call needed.</summary>
        public event EventHandler<WinsockDataArrivalEventArgs>? DataArrival;

        /// <summary>A complete text line has arrived (LF terminated, trailing CR removed). Useful for telnet-style protocols.</summary>
        public event EventHandler<WinsockLineReceivedEventArgs>? LineReceived;

        /// <summary>A queued send has been written to the socket (VB6: SendComplete).</summary>
        public event EventHandler<WinsockSendCompleteEventArgs>? SendComplete;

        /// <summary>A socket error occurred (VB6: Error). After this, State is Error and Connect can be called again.</summary>
        public event EventHandler<WinsockErrorEventArgs>? ErrorReceived;

        /// <summary>The value of <see cref="State"/> has changed.</summary>
        public event EventHandler? StateChanged;

        #endregion

        #region Properties

        /// <summary>Host passed to the last Connect call.</summary>
        public string RemoteHost { get; private set; } = string.Empty;

        /// <summary>Port passed to the last Connect call.</summary>
        public int RemotePort { get; private set; }

        /// <summary>Current connection state.</summary>
        public WinsockState State
        {
            get { lock (_sync) { return _state; } }
        }

        /// <summary>Text encoding used by Send(string), DataArrival.Text and LineReceived. Read when Connect is called.</summary>
        public Encoding Encoding { get; set; } = Encoding.Latin1;

        /// <summary>Maximum time allowed for a connection attempt. On expiry ErrorReceived is raised with SocketError.TimedOut.</summary>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(15);

        /// <summary>Pause inserted after each queued send (TimeSpan.Zero = none).</summary>
        public TimeSpan SendInterval { get; set; } = TimeSpan.Zero;

        /// <summary>Size of the receive buffer, in bytes.</summary>
        public int ReceiveBufferSize { get; set; } = 8192;

        #endregion

        #region Public methods

        /// <summary>
        /// Starts connecting and returns immediately (VB6 style). The outcome arrives through
        /// <see cref="Connected"/> or <see cref="ErrorReceived"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">The client is not Closed or in Error.</exception>
        public void Connect(string remoteHost, int remotePort)
        {
            StartConnect(remoteHost, remotePort);
        }

        /// <summary>
        /// Connects and waits for the outcome. Returns true when connected, false when the attempt
        /// failed (the error is also reported through <see cref="ErrorReceived"/>) or was closed.
        /// </summary>
        public Task<bool> ConnectAsync(string remoteHost, int remotePort)
        {
            return StartConnect(remoteHost, remotePort).ConnectResult.Task;
        }

        /// <summary>Queues text for sending, encoded with <see cref="Encoding"/>.</summary>
        /// <exception cref="InvalidOperationException">The client is not connected.</exception>
        public void Send(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            Session s = GetConnectedSession();
            Enqueue(s, s.Encoding.GetBytes(text));
        }

        /// <summary>Queues raw bytes for sending.</summary>
        /// <exception cref="InvalidOperationException">The client is not connected.</exception>
        public void Send(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Session s = GetConnectedSession();
            Enqueue(s, (byte[])data.Clone());
        }

        /// <summary>
        /// Closes the connection (or cancels a pending connection attempt) and returns to Closed.
        /// Safe to call in any state. Data still queued for sending is discarded.
        /// </summary>
        public void Close()
        {
            Session? s;
            lock (_sync)
            {
                s = _session;
            }

            if (s == null)
            {
                SetState(WinsockState.Closed);
                return;
            }

            SetState(WinsockState.Closing);
            EndSession(s, WinsockState.Closed, null);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Close();
            _disposed = true;
        }

        #endregion

        #region Connection lifecycle

        private Session StartConnect(string remoteHost, int remotePort)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(WinsockClient));
            if (string.IsNullOrWhiteSpace(remoteHost)) throw new ArgumentException("Remote host is required.", nameof(remoteHost));
            if (remotePort < 1 || remotePort > 65535) throw new ArgumentOutOfRangeException(nameof(remotePort));

            Session session;
            lock (_sync)
            {
                if (_state != WinsockState.Closed && _state != WinsockState.Error)
                    throw new InvalidOperationException($"Cannot connect while the state is {_state}. Call Close first.");

                RemoteHost = remoteHost;
                RemotePort = remotePort;
                session = new Session(Encoding);
                _session = session;
            }

            SetState(WinsockState.Connecting);
            _ = RunAsync(session, remoteHost, remotePort, ConnectTimeout);
            return session;
        }

        private async Task RunAsync(Session s, string host, int port, TimeSpan connectTimeout)
        {
            try
            {
                using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(s.Cts.Token))
                {
                    timeout.CancelAfter(connectTimeout);
                    try
                    {
                        await s.Client.ConnectAsync(host, port, timeout.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (!s.Cts.IsCancellationRequested)
                    {
                        throw new SocketException((int)SocketError.TimedOut);
                    }
                }

                s.Stream = s.Client.GetStream();

                lock (_sync)
                {
                    if (!ReferenceEquals(_session, s)) return; // closed while connecting
                    s.WasConnected = true;
                }

                SetState(WinsockState.Connected);
                s.ConnectResult.TrySetResult(true);
                Raise(() => Connected?.Invoke(this, EventArgs.Empty));

                Task sending = SendLoopAsync(s);
                Task receiving = ReceiveLoopAsync(s);
                await Task.WhenAll(sending, receiving).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                HandleFailure(s, ex);
            }
        }

        private async Task ReceiveLoopAsync(Session s)
        {
            try
            {
                var buffer = new byte[Math.Max(512, ReceiveBufferSize)];
                Decoder decoder = s.Encoding.GetDecoder();
                var chars = new char[s.Encoding.GetMaxCharCount(buffer.Length)];
                var line = new StringBuilder();

                while (true)
                {
                    int read = await s.Stream!.ReadAsync(buffer.AsMemory(), s.Cts.Token).ConfigureAwait(false);
                    if (read == 0)
                    {
                        // Remote host closed the connection gracefully.
                        EndSession(s, WinsockState.Closed, null);
                        return;
                    }

                    byte[] data = buffer.AsSpan(0, read).ToArray();
                    int charCount = decoder.GetChars(buffer, 0, read, chars, 0, false);
                    string text = new string(chars, 0, charCount);

                    var dataArgs = new WinsockDataArrivalEventArgs(data, text);
                    Raise(() => DataArrival?.Invoke(this, dataArgs));

                    SplitLines(text, line);
                }
            }
            catch (Exception ex)
            {
                HandleFailure(s, ex);
            }
        }

        private void SplitLines(string text, StringBuilder pending)
        {
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    if (pending.Length > 0 && pending[pending.Length - 1] == '\r')
                        pending.Length--;
                    EmitLine(pending);
                }
                else
                {
                    pending.Append(c);
                    if (pending.Length >= MaxLineLength)
                        EmitLine(pending);
                }
            }
        }

        private void EmitLine(StringBuilder pending)
        {
            var args = new WinsockLineReceivedEventArgs(pending.ToString());
            pending.Clear();
            Raise(() => LineReceived?.Invoke(this, args));
        }

        private async Task SendLoopAsync(Session s)
        {
            try
            {
                await foreach (byte[] payload in s.Outgoing.Reader.ReadAllAsync(s.Cts.Token).ConfigureAwait(false))
                {
                    await s.Stream!.WriteAsync(payload.AsMemory(), s.Cts.Token).ConfigureAwait(false);

                    var args = new WinsockSendCompleteEventArgs(payload.Length);
                    Raise(() => SendComplete?.Invoke(this, args));

                    TimeSpan interval = SendInterval;
                    if (interval > TimeSpan.Zero)
                        await Task.Delay(interval, s.Cts.Token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                HandleFailure(s, ex);
            }
        }

        private void HandleFailure(Session s, Exception ex)
        {
            if (s.Cts.IsCancellationRequested)
            {
                // Expected: Close() (or another failure) already ended this session.
                EndSession(s, WinsockState.Closed, null);
                return;
            }

            EndSession(s, WinsockState.Error, ToErrorArgs(ex));
        }

        /// <summary>Tears a session down exactly once and raises the resulting events.</summary>
        private void EndSession(Session s, WinsockState finalState, WinsockErrorEventArgs? error)
        {
            if (Interlocked.Exchange(ref s.Ended, 1) == 1) return;

            s.Cts.Cancel();
            s.Outgoing.Writer.TryComplete();
            try { s.Client.Dispose(); } catch { /* already gone */ }

            try
            {
                bool wasConnected;
                lock (_sync)
                {
                    if (!ReferenceEquals(_session, s)) return; // a newer session has replaced this one
                    _session = null;
                    wasConnected = s.WasConnected;
                }

                SetState(finalState);

                if (wasConnected)
                    Raise(() => Disconnected?.Invoke(this, EventArgs.Empty));

                if (error != null)
                    Raise(() => ErrorReceived?.Invoke(this, error));
            }
            finally
            {
                // Completed last, so ConnectAsync callers observe the final State.
                s.ConnectResult.TrySetResult(false);
            }
        }

        #endregion

        #region Helpers

        private Session GetConnectedSession()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(WinsockClient));
            lock (_sync)
            {
                if (_state != WinsockState.Connected || _session == null)
                    throw new InvalidOperationException("The socket is not connected.");
                return _session;
            }
        }

        private static void Enqueue(Session s, byte[] payload)
        {
            if (!s.Outgoing.Writer.TryWrite(payload))
                throw new InvalidOperationException("The socket is closing.");
        }

        private void SetState(WinsockState newState)
        {
            bool changed;
            lock (_sync)
            {
                changed = _state != newState;
                _state = newState;
            }

            if (changed)
                Raise(() => StateChanged?.Invoke(this, EventArgs.Empty));
        }

        /// <summary>
        /// Runs an event invocation on the captured context. Handlers are looked up when the
        /// callback runs, so handlers removed in the meantime are not called.
        /// </summary>
        private void Raise(Action invoke)
        {
            if (_context != null)
                _context.Post(static state => ((Action)state!).Invoke(), invoke);
            else
                invoke();
        }

        private static WinsockErrorEventArgs ToErrorArgs(Exception ex)
        {
            for (Exception? e = ex; e != null; e = e.InnerException)
            {
                if (e is SocketException se)
                    return new WinsockErrorEventArgs(se.SocketErrorCode, se.Message, ex);
            }

            SocketError code = ex is IOException ? SocketError.ConnectionReset : SocketError.SocketError;
            return new WinsockErrorEventArgs(code, ex.Message, ex);
        }

        private sealed class Session
        {
            public Session(Encoding encoding)
            {
                Encoding = encoding;
            }

            public readonly TcpClient Client = new TcpClient { NoDelay = true };
            public readonly CancellationTokenSource Cts = new CancellationTokenSource();
            public readonly Channel<byte[]> Outgoing = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions { SingleReader = true });
            public readonly TaskCompletionSource<bool> ConnectResult = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            public readonly Encoding Encoding;
            public NetworkStream? Stream;
            public bool WasConnected;
            public int Ended;
        }

        #endregion
    }
}
