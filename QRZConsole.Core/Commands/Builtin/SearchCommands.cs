using System.Linq;
using System.Threading.Tasks;
using QRZConsole.Core.Models;

namespace QRZConsole.Core.Commands.Builtin
{
    /// <summary>Shared implementation of lookup and check-worked (used by lu, cw, lw, wl).</summary>
    internal static class Search
    {
        /// <summary>Uses the argument, or the current QRZ when empty; updates the current QRZ.</summary>
        private static string ResolveCall(CommandContext ctx, string argument)
        {
            string call = argument.Trim().ToUpperInvariant();
            if (call.Length == 0)
                return ctx.Session.CurrentQrz;

            ctx.Session.CurrentQrz = call;
            return call;
        }

        public static async Task LookupAsync(CommandContext ctx, string argument)
        {
            string call = ResolveCall(ctx, argument);
            if (call.Length == 0)
            {
                ctx.Out.WriteLine("No QRZ to search");
                return;
            }

            ctx.Settings.LastLookup = call;
            ctx.Out.WriteLine();
            ctx.Out.WriteLine($"Lookup {call}...");

            LookupEntry? entry = await ctx.Logbook.LookupAsync(call);
            if (entry == null)
            {
                ctx.Out.WriteLine("Unable to complete the lookup (are you logged in?)");
                return;
            }

            string locatorWorked = string.Empty;
            if (entry.GridSquare.Length >= 4 && ctx.Session.Locators.Count > 0)
            {
                string square = entry.GridSquare.Substring(0, 4);
                var match = ctx.Session.Locators.Where(x => x.Key == square).ToList();
                if (match.Count > 0)
                    locatorWorked = $"  (Worked and {(match[0].Value ? "Confirmed" : "Unconfirmed")})";
            }

            ctx.Out.WriteLine("Result = --->");
            ctx.Out.WriteLine($"   QRZ          = {entry.QRZ}");
            ctx.Out.WriteLine($"   Name         = {entry.Name}");
            ctx.Out.WriteLine($"   DXCC         = {entry.DXCC}");
            ctx.Out.WriteLine($"   Country      = {entry.Country}");
            ctx.Out.WriteLine($"   Grid Locator = {entry.GridSquare} {locatorWorked}");
            ctx.Out.WriteLine($"   Address 1    = {entry.Address1}");
            ctx.Out.WriteLine($"   Address 2    = {entry.Address2}");
            if (entry.Address3.Length > 0) ctx.Out.WriteLine($"   Address 3    = {entry.Address3}");
            if (entry.UsState.Length > 0) ctx.Out.WriteLine($"   US State     = {entry.UsState}");
            if (entry.UsCounty.Length > 0) ctx.Out.WriteLine($"   US County    = {entry.UsCounty}");
            if (entry.Distance.Length > 0) ctx.Out.WriteLine($"   Distance     = {entry.Distance}");
            ctx.Out.WriteLine($"   Email        = {entry.Email}");
            ctx.Out.WriteLine("------------------------------------");
        }

        public static async Task CheckWorkedAsync(CommandContext ctx, string argument)
        {
            string call = ResolveCall(ctx, argument);
            if (call.Length == 0)
            {
                ctx.Out.WriteLine("No QRZ to check");
                return;
            }

            ctx.Settings.LastLookup = call;
            ctx.Out.WriteLine();
            ctx.Out.WriteLine($"Check Worked {call}...");

            string result = await ctx.Logbook.CheckWorkedRawAsync(call);
            ctx.Out.WriteLine(result.Trim().Length > 0 ? result.TrimEnd() : "No result found!");
        }
    }

    public sealed class LookupCommand : Command
    {
        public override string Name => "lu";
        public override string Group => CommandGroups.Search;
        public override string Description => "Lookup";
        public override string Usage => "lu [qrz]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line) => Search.LookupAsync(ctx, line.Arg(0));
    }

    public sealed class CheckWorkedCommand : Command
    {
        public override string Name => "cw";
        public override string Group => CommandGroups.Search;
        public override string Description => "Check Worked";
        public override string Usage => "cw [qrz]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line) => Search.CheckWorkedAsync(ctx, line.Arg(0));
    }

    public sealed class LookupAndCheckWorkedCommand : Command
    {
        public override string Name => "lw";
        public override string Group => CommandGroups.Search;
        public override string Description => "Lookup and Check Worked";
        public override string Usage => "lw [qrz]";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            await Search.LookupAsync(ctx, line.Arg(0));
            await Search.CheckWorkedAsync(ctx, line.Arg(0));
        }
    }

    public sealed class CheckWorkedAndLookupCommand : Command
    {
        public override string Name => "wl";
        public override string Group => CommandGroups.Search;
        public override string Description => "Check Worked and Lookup";
        public override string Usage => "wl [qrz]";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            await Search.CheckWorkedAsync(ctx, line.Arg(0));
            await Search.LookupAsync(ctx, line.Arg(0));
        }
    }
}
