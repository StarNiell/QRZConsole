using QRZConsole.Core.Cluster;
using QRZConsole.Core.Services;
using QRZConsole.Core.Session;
using QRZConsole.Core.Settings;

namespace QRZConsole.Core.Commands
{
    /// <summary>Everything a command can use.</summary>
    public sealed class CommandContext
    {
        public CommandContext(IConsoleHost host, ILogbookService logbook, SessionState session,
                              AppSettings settings, DxClusterService cluster, CommandCatalog catalog)
        {
            Host = host;
            Logbook = logbook;
            Session = session;
            Settings = settings;
            Cluster = cluster;
            Catalog = catalog;
        }

        public IConsoleHost Host { get; }

        /// <summary>Shortcut for Host as output.</summary>
        public IConsoleOutput Out => Host;

        public ILogbookService Logbook { get; }
        public SessionState Session { get; }
        public AppSettings Settings { get; }
        public DxClusterService Cluster { get; }
        public CommandCatalog Catalog { get; }
    }
}
