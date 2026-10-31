using QRZConsole.Core.Cluster;
using QRZConsole.Tests.Fakes;
using QRZNetwork;

namespace QRZConsole.Tests
{
    public class DxClusterServiceTests
    {
        private static DxClusterService Create(RecordingHost host, FakeDxSpider server)
        {
            return new DxClusterService(host, new WinsockClient(null)) { Host = "127.0.0.1", Port = server.Port };
        }

        [Fact]
        public async Task Logs_in_configures_the_session_and_becomes_ready()
        {
            using var server = new FakeDxSpider();
            var host = new RecordingHost();
            using DxClusterService cluster = Create(host, server);

            cluster.Open("IU8NQI");
            await Wait.UntilAsync(() => cluster.IsReady, "cluster ready");

            Assert.Equal("IU8NQI", server.Received[0]);
            Assert.Contains("set/prompt ClusterDX>", server.Received);
            Assert.Equal("unset/wx", server.Received.Last());
            Assert.Contains("Cluster DX Logged in", host.Lines);
            // nothing from the login phase is printed (no "login:", no "WX disabled")
            Assert.DoesNotContain(host.Lines, l => l.Contains("WX disabled"));
        }

        [Fact]
        public async Task Prints_cluster_output_without_prompt_lines()
        {
            using var server = new FakeDxSpider();
            var host = new RecordingHost();
            using DxClusterService cluster = Create(host, server);
            cluster.Open("IU8NQI");
            await Wait.UntilAsync(() => cluster.IsReady, "cluster ready");

            Assert.True(cluster.Send("show/dx 5 on 40m"));
            await Wait.UntilAsync(() => host.Lines.Contains(FakeDxSpider.Spot), "spot line");

            Assert.DoesNotContain("ClusterDX>", host.Lines);
        }

        [Fact]
        public async Task Spot_reports_send_complete_and_strips_prompt_prefix()
        {
            using var server = new FakeDxSpider();
            var host = new RecordingHost();
            using DxClusterService cluster = Create(host, server);
            cluster.Open("IU8NQI");
            await Wait.UntilAsync(() => cluster.IsReady, "cluster ready");

            Assert.True(cluster.SendSpot("IU0ABC", "7076", "test"));
            await Wait.UntilAsync(() => host.Lines.Contains("Cluster DX Send Complete"), "send complete");
            await Wait.UntilAsync(() => host.Lines.Any(l => l.StartsWith("DX de IU8NQI")), "spot echo");
            Assert.Contains("DX IU0ABC 7076 test", server.Received);
        }

        [Fact]
        public async Task Close_reports_disconnection_and_resets_ready()
        {
            using var server = new FakeDxSpider();
            var host = new RecordingHost();
            using DxClusterService cluster = Create(host, server);
            cluster.Open("IU8NQI");
            await Wait.UntilAsync(() => cluster.IsReady, "cluster ready");

            cluster.Close();
            await Wait.UntilAsync(() => host.Lines.Any(l => l.StartsWith("Cluster DX disconnected")), "disconnected");

            Assert.False(cluster.IsReady);
            Assert.Equal(WinsockState.Closed, cluster.State);
        }

        [Fact]
        public async Task Refused_connection_is_reported_without_retrying()
        {
            var host = new RecordingHost();
            int closedPort;
            using (var probe = new FakeDxSpider()) closedPort = probe.Port;

            using var cluster = new DxClusterService(host, new WinsockClient(null)) { Host = "127.0.0.1", Port = closedPort };
            cluster.Open("IU8NQI");

            await Wait.UntilAsync(() => host.Lines.Any(l => l.StartsWith("#Error")), "error");
            Assert.DoesNotContain(host.Lines, l => l.Contains("try to reconnect"));
        }
    }
}
