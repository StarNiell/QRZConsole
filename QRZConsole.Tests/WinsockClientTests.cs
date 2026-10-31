using System.Net;
using System.Net.Sockets;
using System.Text;
using QRZNetwork;

namespace QRZConsole.Tests
{
    public class WinsockClientTests
    {
        [Fact]
        public async Task Lines_split_across_packets_are_reassembled()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;

            var lines = new List<string>();
            using var client = new WinsockClient(null);
            client.LineReceived += (s, e) => { lock (lines) lines.Add(e.Line); };

            Assert.True(await client.ConnectAsync("127.0.0.1", port));
            using TcpClient server = await listener.AcceptTcpClientAsync();
            NetworkStream stream = server.GetStream();

            await stream.WriteAsync(Encoding.Latin1.GetBytes("first li"));
            await Task.Delay(50);
            await stream.WriteAsync(Encoding.Latin1.GetBytes("ne\r\ncaffè\nlast"));
            await Task.Delay(50);
            await stream.WriteAsync(Encoding.Latin1.GetBytes("\r\n"));

            await Fakes.Wait.UntilAsync(() => { lock (lines) return lines.Count == 3; }, "three lines");
            Assert.Equal(new[] { "first line", "caffè", "last" }, lines);
            listener.Stop();
        }

        [Fact]
        public async Task Sends_are_queued_in_order()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;

            using var client = new WinsockClient(null) { SendInterval = TimeSpan.FromMilliseconds(5) };
            Assert.True(await client.ConnectAsync("127.0.0.1", port));
            using TcpClient server = await listener.AcceptTcpClientAsync();

            for (int i = 0; i < 20; i++)
                client.Send($"{i}\n");

            var reader = new StreamReader(server.GetStream(), Encoding.Latin1);
            for (int i = 0; i < 20; i++)
                Assert.Equal(i.ToString(), await reader.ReadLineAsync());
            listener.Stop();
        }

        [Fact]
        public async Task Refused_connection_ends_in_error_state_and_can_retry()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            SocketError? error = null;
            using var client = new WinsockClient(null);
            client.ErrorReceived += (s, e) => error = e.ErrorCode;

            Assert.False(await client.ConnectAsync("127.0.0.1", port));
            Assert.Equal(WinsockState.Error, client.State);
            Assert.Equal(SocketError.ConnectionRefused, error);

            var again = new TcpListener(IPAddress.Loopback, 0);
            again.Start();
            Assert.True(await client.ConnectAsync("127.0.0.1", ((IPEndPoint)again.LocalEndpoint).Port));
            again.Stop();
        }

        [Fact]
        public async Task Remote_close_raises_disconnected()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;

            bool disconnected = false;
            using var client = new WinsockClient(null);
            client.Disconnected += (s, e) => disconnected = true;

            Assert.True(await client.ConnectAsync("127.0.0.1", port));
            (await listener.AcceptTcpClientAsync()).Dispose();

            await Fakes.Wait.UntilAsync(() => disconnected, "disconnected");
            Assert.Equal(WinsockState.Closed, client.State);
            listener.Stop();
        }

        [Fact]
        public void Send_when_not_connected_throws()
        {
            using var client = new WinsockClient(null);
            Assert.Throws<InvalidOperationException>(() => client.Send("x"));
        }
    }
}
