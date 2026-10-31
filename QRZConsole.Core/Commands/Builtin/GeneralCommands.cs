using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRZConsole.Core.Data;
using QRZConsole.Core.Formatting;

namespace QRZConsole.Core.Commands.Builtin
{
    public sealed class ClearCommand : Command
    {
        public override string Name => "cl";
        public override string Group => CommandGroups.General;
        public override string Description => "Clear Monitor";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Host.Clear();
            return Task.CompletedTask;
        }
    }

    public sealed class FindDxccCommand : Command
    {
        public override string Name => "fc";
        public override string Group => CommandGroups.General;
        public override string Description => "Find DXCC Countries";
        public override string Usage => "fc [search]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Out.WriteLine("Search DXCC by country...");
            IReadOnlyList<KeyValuePair<int, string>> found = HamData.SearchDxcc(line.RestFrom(0));
            foreach (KeyValuePair<int, string> c in found)
                ctx.Out.WriteLine($"{c.Key.ToString().PadLeft(4)} {c.Value}");
            if (found.Count > 0)
                ctx.Out.WriteLine();
            ctx.Out.WriteLine($"Found {found.Count} DXCC");
            return Task.CompletedTask;
        }
    }

    public sealed class LoadLocatorsCommand : Command
    {
        public override string Name => "lc";
        public override string Group => CommandGroups.General;
        public override string Description => "Load Your Grid Locator";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Out.WriteLine("Load Grid Locators Worked... it's no longer available!");
            return Task.CompletedTask;
        }
    }

    public sealed class LocatorListCommand : Command
    {
        public override string Name => "gl";
        public override string Group => CommandGroups.General;
        public override string Description => "Get Grid Locator List";
        public override string Usage => "gl [loc] [c or u] (example: gl [*] [c] - gl [EN70]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            string locator = line.Arg(0).Trim().ToUpperInvariant();
            string filter = line.Arg(1).Trim();

            IEnumerable<KeyValuePair<string, bool>> list = ctx.Session.Locators;
            if (locator.Length > 0 && locator != "*")
                list = list.Where(x => x.Key.StartsWith(locator));
            if (filter == "c")
                list = list.Where(x => x.Value);
            else if (filter == "u")
                list = list.Where(x => !x.Value);

            List<KeyValuePair<string, bool>> result = list.ToList();
            if (result.Count == 0)
            {
                ctx.Out.WriteLine("0 items Found (try to load your Grid Locator List Worked using the [lc] command)");
                return Task.CompletedTask;
            }

            foreach (KeyValuePair<string, bool> l in result)
                ctx.Out.WriteLine($"{l.Key}   {(l.Value ? "Confirmed" : "Unconfirmed")}");
            ctx.Out.WriteLine();
            ctx.Out.WriteLine($"{result.Count} Items found");
            return Task.CompletedTask;
        }
    }

    public sealed class SetFrequencyCommand : Command
    {
        public override string Name => "ff";
        public override string Group => CommandGroups.General;
        public override string Description => "Set current Frequency";
        public override string Usage => "ff [freq]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            string freq = line.Arg(0);
            if (freq.Length <= 3)
            {
                ctx.Out.WriteLine($"Invalid frequency! Usage: {Usage} (e.g. ff 7076)");
                return Task.CompletedTask;
            }

            ctx.Session.LastFrequency = freq.Replace(".", "");
            ctx.Settings.LastFrequency = freq;
            ctx.Out.WriteLine();
            ctx.Out.WriteLine($"Current Frequency: {freq}");
            return Task.CompletedTask;
        }
    }

    public sealed class SetQrzCommand : Command
    {
        public override string Name => "qq";
        public override string Group => CommandGroups.General;
        public override string Description => "Set current QRZ";
        public override string Usage => "qq [qrz]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            string call = line.Arg(0).ToUpperInvariant();
            ctx.Settings.LastLookup = call;
            ctx.Session.CurrentQrz = call;
            ctx.Out.WriteLine();
            ctx.Out.WriteLine($"Current QRZ: {call}");
            return Task.CompletedTask;
        }
    }

    public sealed class QrzHomeCommand : Command
    {
        public override string Name => "hm";
        public override string Group => CommandGroups.General;
        public override string Description => "Load QRZ.com home page";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Out.WriteLine("Load QRZ Home...");
            ctx.Out.WriteLine($"Load QRZ Home = {await ctx.Logbook.GotoQrzHomeAsync()}");
        }
    }

    public sealed class SwitchViewCommand : Command
    {
        public override string Name => "sw";
        public override string Group => CommandGroups.General;
        public override string Description => "Collapse / expand the ribbon";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Host.ToggleHeader();
            return Task.CompletedTask;
        }
    }

    public sealed class SwitchCheckLoggedCommand : Command
    {
        public override string Name => "sc";
        public override string Group => CommandGroups.General;
        public override string Description => "Switch Check Is Logged at startup";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Host.ToggleCheckIsLoggedAtStartup();
            return Task.CompletedTask;
        }
    }

    public sealed class FullScreenCommand : Command
    {
        public override string Name => "fs";
        public override string Group => CommandGroups.General;
        public override string Description => "Switch screen (normal/fullsize)";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Host.ToggleFullScreen();
            return Task.CompletedTask;
        }
    }

    public sealed class LastDataCommand : Command
    {
        public override string Name => "ld";
        public override string Group => CommandGroups.General;
        public override string Description => "Last data in memory";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            const int width = 20;
            ctx.Out.WriteLine("Last data in memory:");
            ctx.Out.WriteLine(Text.Fixed("  QRZ", width) + ctx.Session.CurrentQrz);
            ctx.Out.WriteLine(Text.Fixed("  Freq.", width) + ctx.Session.LastFrequency);
            ctx.Out.WriteLine(Text.Fixed("  Mode", width) + ctx.Session.LastMode);
            return Task.CompletedTask;
        }
    }

    public sealed class CommandListCommand : Command
    {
        public override string Name => "cm";
        public override string Group => CommandGroups.General;
        public override string Description => "Command List";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Catalog.WriteCommandList(ctx.Out);
            return Task.CompletedTask;
        }
    }

    public sealed class ShortcutListCommand : Command
    {
        public override string Name => "sl";
        public override string Group => CommandGroups.General;
        public override string Description => "Shortcut List";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            const int width = 40;
            string rule = new string('-', 73);
            ctx.Out.WriteLine("Shortcut List");
            ctx.Out.WriteLine(rule);
            ctx.Out.WriteLine(Text.Fixed("  Description", width) + "Keys...");
            ctx.Out.WriteLine(rule);
            ctx.Out.WriteLine("View:");
            ctx.Out.WriteLine(Text.Fixed("  Zoom in", width) + "CTRL++");
            ctx.Out.WriteLine(Text.Fixed("  Zoom out", width) + "CTRL+-");
            ctx.Out.WriteLine(Text.Fixed("  Zoom reset", width) + "CTRL+0");
            ctx.Out.WriteLine("Logbook:");
            ctx.Out.WriteLine(Text.Fixed("  Page Up (pu)", width) + "CTRL+PagUp");
            ctx.Out.WriteLine(Text.Fixed("  Page Down (pd)", width) + "CTRL+PagDown");
            ctx.Out.WriteLine("Behaviors:");
            ctx.Out.WriteLine(Text.Fixed("  Back to the command line", width) + "F2");
            ctx.Out.WriteLine(Text.Fixed("  Moving back the command list", width) + "Arrow Key Up");
            ctx.Out.WriteLine(Text.Fixed("  Moving forward the command list", width) + "Arrow Key Down");
            ctx.Out.WriteLine(Text.Fixed("  Next / previous parameter", width) + "TAB / SHIFT+TAB");
            ctx.Out.WriteLine(Text.Fixed("  Clear the command line", width) + "ESC");
            ctx.Out.WriteLine(Text.Fixed("  Collapse / expand the ribbon", width) + "CTRL+F1");
            ctx.Out.WriteLine(rule);
            return Task.CompletedTask;
        }
    }

    public sealed class ThemeCommand : Command
    {
        public override string Name => "th";
        public override string Group => CommandGroups.General;
        public override string Description => "Console theme";
        public override string Usage => "th [theme name | list] (no parameter: open the theme editor)";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            string name = line.RestFrom(0);
            if (name.Length == 0)
            {
                ctx.Host.ShowThemeEditor();
            }
            else if (name.Equals("list", System.StringComparison.OrdinalIgnoreCase))
            {
                ctx.Out.WriteLine("Themes: " + string.Join(", ", ctx.Host.ThemeNames));
            }
            else if (ctx.Host.ApplyTheme(name))
            {
                ctx.Out.WriteLine($"Theme set: {name}");
            }
            else
            {
                ctx.Out.WriteLine($"Unknown theme \"{name}\". Themes: " + string.Join(", ", ctx.Host.ThemeNames));
            }
            return Task.CompletedTask;
        }
    }

    public sealed class QuitCommand : Command
    {
        public override string Name => "qi";
        public override IReadOnlyList<string> Aliases => new[] { "exit" };
        public override string Group => CommandGroups.General;
        public override string Description => "Quit";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Host.Quit();
            return Task.CompletedTask;
        }
    }
}

namespace QRZConsole.Core.Commands.Builtin
{
    public sealed class BrowserDebugCommand : Command
    {
        public override string Name => "bd";
        public override string Group => CommandGroups.General;
        public override string Description => "Switch browser (scraper) debug";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Logbook.Verbose = !ctx.Logbook.Verbose;
            ctx.Out.WriteLine($"Browser debug active: {ctx.Logbook.Verbose}");
            return Task.CompletedTask;
        }
    }

    public sealed class BrowserFieldsCommand : Command
    {
        public override string Name => "bf";
        public override string Group => CommandGroups.General;
        public override string Description => "Show form fields of the current page";
        public override string Usage => "bf [text] (only fields containing the text, e.g. bf comment)";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Out.WriteLine(await ctx.Logbook.DescribeCurrentPageAsync());
            string filter = line.RestFrom(0);
            IReadOnlyList<string> fields = (await ctx.Logbook.DescribeFormFieldsAsync())
                .Where(f => filter.Length == 0 || f.Contains(filter, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (string f in fields)
                ctx.Out.WriteLine("  " + f);
            ctx.Out.WriteLine($"Found {fields.Count} fields");
        }
    }
}
