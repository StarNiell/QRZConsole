using System.Threading.Tasks;

namespace QRZConsole.Core.Commands.Builtin
{
    internal static class ClusterShow
    {
        public static void ShowDx(CommandContext ctx, string band, int items)
        {
            if (items <= 0) items = ctx.Cluster.Items;
            ctx.Cluster.Send(band.Length > 0 ? $"show/dx {items} on {band}" : $"show/dx {items}");
        }
    }

    public sealed class ClusterOpenCommand : Command
    {
        public override string Name => "dx";
        public override string Group => CommandGroups.ClusterDx;
        public override string Description => "Open Cluster DX";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            if (ctx.Session.Username.Length == 0)
                ctx.Out.WriteLine("Please! Before connecting to the Cluster DX, login in QRZ.com using: li [username] [password] command");
            else
                ctx.Cluster.Open(ctx.Session.Username);
            return Task.CompletedTask;
        }
    }

    public sealed class ClusterShowBandCommand : Command
    {
        public override string Name => "sb";
        public override string Group => CommandGroups.ClusterDx;
        public override string Description => "Show DX on Band";
        public override string Usage => "sb [band] [items] (band example: [40m] [10m] or [hf] [vhf] [uhf])";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ClusterShow.ShowDx(ctx, line.Arg(0), line.IntArg(1));
            return Task.CompletedTask;
        }
    }

    public sealed class ClusterShowFrequencyCommand : Command
    {
        public override string Name => "sf";
        public override string Group => CommandGroups.ClusterDx;
        public override string Description => "Show DX on Freq";
        public override string Usage => "sf [freq] [items] (freq example: [7155] [14200] ...)";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            string freq = line.Arg(0).Length > 0 ? line.Arg(0) : ctx.Session.LastFrequency;
            ClusterShow.ShowDx(ctx, $"{freq}/{freq}", line.IntArg(1));
            return Task.CompletedTask;
        }
    }

    public sealed class ClusterSearchCallCommand : Command
    {
        public override string Name => "ds";
        public override string Group => CommandGroups.ClusterDx;
        public override string Description => "Search Callsign on Cluster DX";
        public override string Usage => "ds [callsign]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Cluster.Send($"show/dx {line.Arg(0)}".TrimEnd());
            return Task.CompletedTask;
        }
    }

    public sealed class ClusterRawCommand : Command
    {
        public override string Name => "dc";
        public override string Group => CommandGroups.ClusterDx;
        public override string Description => "DXSpider command";
        public override string Usage => "dc [DXSpider command: http://www.dxcluster.org/main/usermanual_en-12.html]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            string command = line.RestFrom(0);
            if (command.Length == 0)
                ctx.Out.WriteLine("Invalid DXSpider command!");
            else
                ctx.Cluster.Send(command);
            return Task.CompletedTask;
        }
    }

    public sealed class ClusterSendSpotCommand : Command
    {
        public override string Name => "ss";
        public override string Group => CommandGroups.ClusterDx;
        public override string Description => "Send Spot";
        public override string Usage => "ss [call] [freq] [comment]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            if (line.Arg(0).Length == 0 || line.Arg(1).Length == 0)
                ctx.Out.WriteLine("Invalid Spot command for Cluster DX!");
            else
                ctx.Cluster.SendSpot(line.Arg(0), line.Arg(1), line.RestFrom(2));
            return Task.CompletedTask;
        }
    }

    public sealed class ClusterItemsCommand : Command
    {
        public override string Name => "di";
        public override string Group => CommandGroups.ClusterDx;
        public override string Description => "Set default spots for sb/sf";
        public override string Usage => "di [items]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            int items = line.IntArg(0);
            if (items > 0)
            {
                ctx.Cluster.Items = items;
                ctx.Out.WriteLine($"Set Items result for Cluster DX: {items}");
            }
            else
            {
                ctx.Out.WriteLine("Invalid Items value for Cluster DX: the number must be greater than 0");
            }
            return Task.CompletedTask;
        }
    }

    public sealed class ClusterDebugCommand : Command
    {
        public override string Name => "cd";
        public override string Group => CommandGroups.ClusterDx;
        public override string Description => "Switch Cluster DX debug";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Cluster.Debug = !ctx.Cluster.Debug;
            ctx.Out.WriteLine($"Cluster Debug active: {ctx.Cluster.Debug}");
            return Task.CompletedTask;
        }
    }

    public sealed class ClusterCloseCommand : Command
    {
        public override string Name => "cc";
        public override string Group => CommandGroups.ClusterDx;
        public override string Description => "Close Cluster DX";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Cluster.Close();
            return Task.CompletedTask;
        }
    }
}
