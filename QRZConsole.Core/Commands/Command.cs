using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QRZConsole.Core.Commands
{
    /// <summary>Command groups, in the order they appear in the command list (cm).</summary>
    public static class CommandGroups
    {
        public const string Credentials = "Credentials";
        public const string Search = "Search";
        public const string Logbook = "Logbook";
        public const string ClusterDx = "Cluster DX";
        public const string General = "General/Utility";

        public static IReadOnlyList<string> Ordered { get; } = new[] { Credentials, Search, Logbook, ClusterDx, General };
    }

    /// <summary>
    /// A console command. To add a command: derive from this class, then register it in
    /// <see cref="CommandCatalog.CreateDefault"/>; the command list (cm) is built from these properties.
    /// </summary>
    public abstract class Command
    {
        /// <summary>What the user types (two letters, lower case).</summary>
        public abstract string Name { get; }

        /// <summary>Other names accepted for the same command.</summary>
        public virtual IReadOnlyList<string> Aliases => Array.Empty<string>();

        public abstract string Group { get; }

        /// <summary>Short description shown in the command list.</summary>
        public abstract string Description { get; }

        /// <summary>Syntax shown in the command list, e.g. "lu [qrz]".</summary>
        public virtual string Usage => Name;

        public abstract Task ExecuteAsync(CommandContext context, CommandLine line);
    }
}
