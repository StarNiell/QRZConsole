using System.Net;
using System.Net.Sockets;
using System.Text;

namespace QRZConsole.Tests.Fakes
{
    /// <summary>
    /// Minimal DXSpider-like telnet server on localhost: asks for login, confirms "unset/wx"
    /// with "WX disabled for", answers show/dx with one spot and echoes DX spots.
    /// </summary>
    internal sealed class FakeDxSpider : IDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _cts = new();
        private readonly List<string> _received = new();

        public FakeDxSpider()
        {
            _listener.Start();
            _ = Task.Run(ServeAsync);
        }

        public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

        public IReadOnlyList<string> Received { get { lock (_received) return _received.ToList(); } }

        public const string Spot = "  7076.0  IU0ABC      31-Oct-2026 0010Z  FT8 test         <IU8NQI>";

        private async Task ServeAsync()
        {
            try
            {
                using TcpClient client = await _listener.AcceptTcpClientAsync(_cts.Token);
                NetworkStream stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.Latin1);
                var writer = new StreamWriter(stream, Encoding.Latin1) { AutoFlush = true, NewLine = "\r\n" };

                await writer.WriteAsync("login: ");
                string? line;
                while ((line = await reader.ReadLineAsync(_cts.Token)) != null)
                {
                    lock (_received) _received.Add(line);

                    if (line == "unset/wx")
                    {
                        await writer.WriteLineAsync("WX disabled for IU8NQI");
                        await writer.WriteLineAsync("ClusterDX>");
                    }
                    else if (line.StartsWith("show/dx", StringComparison.Ordinal))
                    {
                        await writer.WriteLineAsync(Spot);
                        await writer.WriteLineAsync("ClusterDX>");
                    }
                    else if (line.StartsWith("DX ", StringComparison.Ordinal))
                    {
                        await writer.WriteLineAsync("ClusterDX>DX de IU8NQI:   7076.0  IU0ABC       test");
                    }
                }
            }
            catch (Exception) when (_cts.IsCancellationRequested)
            {
            }
            catch (IOException)
            {
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _listener.Stop();
        }
    }
}
