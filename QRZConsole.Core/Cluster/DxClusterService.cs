using System;
using QRZConsole.Core.Commands;
using QRZNetwork;

namespace QRZConsole.Core.Cluster
{
    /// <summary>
    /// Connection to a DXSpider cluster over telnet: connects, logs in with the callsign,
    /// configures the session and prints what the cluster sends.
    /// </summary>
    /// <remarks>
    /// Login is a small state machine driven by complete lines (<see cref="WinsockClient.LineReceived"/>):
    /// after connecting, the setup commands are queued; the last one is "unset/wx", whose
    /// confirmation ("WX disabled for ...") marks the session as ready.
    /// Create the service on the UI thread: output then happens on the UI thread too.
    /// </remarks>
    public sealed class DxClusterService : IDisposable
    {
        private const string LoggedInMarker = "WX disabled for ";
        private const int MaxReconnectAttempts = 3;
        private const string NotReadyMessage = "Cluster DX not available. Enter dx to log in to the Cluster DX";

        private readonly IConsoleOutput _output;
        private readonly WinsockClient _socket;
        private bool _spotPending;
        private int _reconnectAttempts;
        private string _callsign = string.Empty;

        public DxClusterService(IConsoleOutput output, WinsockClient? socket = null)
        {
            _output = output;
            _socket = socket ?? new WinsockClient();
            _socket.SendInterval = TimeSpan.FromMilliseconds(50);

            _socket.Connected += OnConnected;
            _socket.Disconnected += OnDisconnected;
            _socket.LineReceived += OnLineReceived;
            _socket.ErrorReceived += OnError;
            _socket.SendComplete += OnSendComplete;
        }

        public string Host { get; set; } = "dxfun.com";

        public int Port { get; set; } = 8000;

        /// <summary>Prompt configured on the cluster (set/prompt); lines equal to it are not printed.</summary>
        public string Prompt { get; set; } = "ClusterDX>";

        /// <summary>When true, everything is printed, including the login phase and sent commands.</summary>
        public bool Debug { get; set; }

        /// <summary>Default number of spots for show/dx.</summary>
        public int Items { get; set; } = 30;

        /// <summary>True once logged in and configured.</summary>
        public bool IsReady { get; private set; }

        public WinsockState State => _socket.State;

        /// <summary>Connects and logs in with the callsign. Does nothing if already connected.</summary>
        public void Open(string callsign)
        {
            _callsign = callsign;
            _reconnectAttempts = 0;
            ConnectIfNeeded();

            if (IsReady)
                _output.WriteLine("Cluster already connected");
        }

        public void Close()
        {
            if (_socket.State == WinsockState.Connected || _socket.State == WinsockState.Connecting)
            {
                _output.WriteLine("Disconnecting Cluster DX....");
                _socket.Close();
            }
            else
            {
                _output.WriteLine("Cluster DX is not connected");
            }
        }

        /// <summary>Sends a DXSpider command. Returns false (and says why) when not logged in.</summary>
        public bool Send(string command)
        {
            if (!IsReady)
            {
                _output.WriteLine(NotReadyMessage);
                return false;
            }

            SendRaw(command);
            return true;
        }

        /// <summary>Sends a spot; "Cluster DX Send Complete" is printed when it has been written.</summary>
        public bool SendSpot(string call, string frequency, string comment)
        {
            if (!IsReady)
            {
                _output.WriteLine(NotReadyMessage);
                return false;
            }

            _output.WriteLine($"Send spot to {call} on freq {frequency}");
            _spotPending = true;
            return Send($"DX {call} {frequency} {comment}".TrimEnd());
        }

        public void Dispose()
        {
            _socket.Connected -= OnConnected;
            _socket.Disconnected -= OnDisconnected;
            _socket.LineReceived -= OnLineReceived;
            _socket.ErrorReceived -= OnError;
            _socket.SendComplete -= OnSendComplete;
            _socket.Dispose();
        }

        private void ConnectIfNeeded()
        {
            if (_socket.State == WinsockState.Closed || _socket.State == WinsockState.Error)
            {
                IsReady = false;
                _socket.Connect(Host, Port);
            }
        }

        private void SendRaw(string command)
        {
            if (Debug)
                _output.WriteLine($"Send: {command}");
            _socket.Send(command + "\n");
        }

        private void OnConnected(object? sender, EventArgs e)
        {
            _reconnectAttempts = 0;
            _output.WriteLine($"Cluster DX connected: {_socket.RemoteHost}:{_socket.RemotePort}");
            _output.WriteLine("Login Cluster DX...");

            SendRaw(_callsign);
            SendRaw($"set/prompt {Prompt}");
            SendRaw("unset/echo");
            SendRaw("unset/announce");
            SendRaw("unset/anntalk");
            SendRaw("unset/dx");
            SendRaw("unset/talk");
            SendRaw("unset/wcy");
            SendRaw("unset/wwv");
            SendRaw("unset/wx"); // its confirmation tells us the setup is complete
        }

        private void OnLineReceived(object? sender, WinsockLineReceivedEventArgs e)
        {
            string line = e.Line;

            if (!IsReady)
            {
                if (Debug)
                    _output.WriteLine(line);

                if (line.Contains(LoggedInMarker, StringComparison.Ordinal))
                {
                    IsReady = true;
                    _output.WriteLine("Cluster DX Logged in");
                }
                return;
            }

            if (line == Prompt)
                return;
            if (line.StartsWith(Prompt, StringComparison.Ordinal))
                line = line.Substring(Prompt.Length);

            _output.WriteLine(line);
        }

        private void OnSendComplete(object? sender, WinsockSendCompleteEventArgs e)
        {
            if (_spotPending)
            {
                _spotPending = false;
                _output.WriteLine("Cluster DX Send Complete");
            }
        }

        private void OnDisconnected(object? sender, EventArgs e)
        {
            IsReady = false;
            _output.WriteLine($"Cluster DX disconnected: {_socket.RemoteHost}:{_socket.RemotePort}");
        }

        private void OnError(object? sender, WinsockErrorEventArgs e)
        {
            IsReady = false;

            if (e.ErrorCode == System.Net.Sockets.SocketError.TimedOut && _reconnectAttempts < MaxReconnectAttempts)
            {
                _reconnectAttempts++;
                _output.WriteLine($"Cluster DX Timeout... try to reconnect ({_reconnectAttempts}/{MaxReconnectAttempts})");
                ConnectIfNeeded();
                return;
            }

            _output.WriteLine($"#Error: {e.Message}");
        }
    }
}
