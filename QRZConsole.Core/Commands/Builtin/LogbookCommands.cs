using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRZConsole.Core.Formatting;
using QRZConsole.Core.Models;
using QRZConsole.Core.Session;

namespace QRZConsole.Core.Commands.Builtin
{
    /// <summary>Shared logbook helpers: page display in the current view mode, QSO argument handling.</summary>
    internal static class LogbookViews
    {
        public static readonly int[] ValidEntriesPerPage = { 5, 10, 15, 20, 25, 50, 100, 200 };

        /// <summary>Page from the argument, or the current page when missing.</summary>
        public static int PageOrCurrent(CommandContext ctx, int page) => page >= 1 ? page : ctx.Session.CurrentPage;

        public static Task ShowPageAsync(CommandContext ctx, int page)
        {
            return ctx.Session.ViewMode switch
            {
                ViewMode.Text => PageTextAsync(ctx, page),
                ViewMode.Adif => PageAdifAsync(ctx, page),
                _ => PageRawAsync(ctx, page)
            };
        }

        public static async Task GotoPageAsync(CommandContext ctx, int page)
        {
            ctx.Out.WriteLine($"Goto Page {page}... ");
            int current = await ctx.Logbook.GotoLogbookPageAsync(page);
            ctx.Out.WriteLine($"Current Page = {current}");

            if (current > 0)
            {
                ctx.Session.CurrentPage = current;
                await ShowPageAsync(ctx, current);
            }
        }

        public static async Task PageRawAsync(CommandContext ctx, int page)
        {
            page = PageOrCurrent(ctx, page);
            ctx.Out.WriteLine($"Get Table content Raw of page {page}... ");
            ctx.Out.WriteLine(await ctx.Logbook.GetLogbookPageContentRawAsync(page));
        }

        public static async Task PageTextAsync(CommandContext ctx, int page)
        {
            page = PageOrCurrent(ctx, page);
            ctx.Out.WriteLine($"Get Table content text of page {page}... ");
            IReadOnlyList<LogbookEntry> entries = await ctx.Logbook.GetLogbookPageContentAsync(page);
            if (entries.Count > 0)
            {
                ctx.Out.WriteLine();
                WriteAll(ctx, LogbookFormatter.PageTable(entries));
            }
            ctx.Out.WriteLine($"Found {entries.Count} entries");
        }

        public static async Task PageAdifAsync(CommandContext ctx, int page)
        {
            page = PageOrCurrent(ctx, page);
            ctx.Out.WriteLine($"Get Table content ADIF of page {page}... ");
            IReadOnlyList<LogbookEntry> entries = await ctx.Logbook.GetLogbookPageContentAsync(page);
            if (entries.Count > 0)
            {
                ctx.Out.WriteLine();
                WriteAll(ctx, LogbookFormatter.Adif(entries));
            }
            ctx.Out.WriteLine();
            ctx.Out.WriteLine($"Found {entries.Count} entries");
        }

        public static async Task PageXmlAsync(CommandContext ctx, int page)
        {
            page = PageOrCurrent(ctx, page);
            ctx.Out.WriteLine($"Get Table content XML of page {page}... ");
            IReadOnlyList<LogbookEntry> entries = await ctx.Logbook.GetLogbookPageContentAsync(page);
            if (entries.Count > 0)
            {
                string stars = new string('*', 96);
                ctx.Out.WriteLine();
                ctx.Out.WriteLine(stars);
                ctx.Out.WriteLine(LogbookFormatter.Xml(entries));
                ctx.Out.WriteLine(stars);
                ctx.Out.WriteLine();
            }
            ctx.Out.WriteLine($"Found {entries.Count} entries");
        }

        public static async Task<IReadOnlyList<LogbookEntry>?> RangeAsync(CommandContext ctx, CommandLine line, string title)
        {
            int start = line.IntArg(0);
            int end = line.IntArg(1);
            if (end == 0) end = start;

            if (start < 1 || end < start)
            {
                ctx.Out.WriteLine($"Invalid range! Usage: {line.Name} [start position] [end position]");
                return null;
            }

            ctx.Out.WriteLine($"Get QSOs by range ({title}): from position {start} to {end}... ");
            return await ctx.Logbook.GetLogbookEntriesByRangeAsync(start, end);
        }

        /// <summary>Accepts kHz ("7076") or MHz ("7.076", "7,076") and returns MHz with a dot.</summary>
        public static string NormalizeFrequency(string frequency)
        {
            string f = frequency.Trim().Replace(',', '.');
            if (f.Length > 3 && !f.Contains('.'))
                f = f.Insert(f.Length - 3, ".");
            return f;
        }

        public static void RememberQso(CommandContext ctx, QsoData qso)
        {
            ctx.Session.LastMode = qso.Mode;
            ctx.Session.CurrentQrz = qso.Call;
            ctx.Session.LastFrequency = qso.Frequency.Replace(".", "");
        }

        public static void WriteQsoFailure(CommandContext ctx, string usage)
        {
            ctx.Out.WriteLine("Unable to save the QSO on QRZ.COM. Please check the command format");
            if (ctx.Logbook.ErrorMessage.Length > 0)
                ctx.Out.WriteLine($"Reason: {ctx.Logbook.ErrorMessage}");
            ctx.Out.WriteLine("Remember that the frequencies must be entered in MHz and contain only one decimal point. (eg 7.076 or 7.07615)");
            ctx.Out.WriteLine(usage);
        }

        public static void WriteAll(CommandContext ctx, IEnumerable<string> lines)
        {
            foreach (string l in lines)
                ctx.Out.WriteLine(l);
        }
    }

    public sealed class AddQsoCommand : Command
    {
        public override string Name => "aq";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Add QSO";
        public override string Usage => "aq [qrz] [freq] [mode] [date] [time] [comment]";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            string call = line.Arg(0).ToUpperInvariant();
            if (call.Length == 0 || call == "[CALL]" || line.Args.Count < 5)
            {
                ctx.Out.WriteLine("Invalid command! Please check the format:");
                ctx.Out.WriteLine(Usage);
                return;
            }

            var qso = new QsoData(call, LogbookViews.NormalizeFrequency(line.Arg(1)), line.Arg(2).ToUpperInvariant(),
                                  line.Arg(3), line.Arg(4), line.RestFrom(5));

            if (await ctx.Logbook.AddQsoAsync(qso))
            {
                LogbookViews.RememberQso(ctx, qso);
                ctx.Out.WriteLine("QSO added successfully!");
            }
            else
            {
                LogbookViews.WriteQsoFailure(ctx, Usage);
            }
        }
    }

    public sealed class EditQsoCommand : Command
    {
        public override string Name => "eq";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Edit QSO";
        public override string Usage => "eq [position] [qrz] [freq] [mode] [date] [time] [comment]";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            int position = line.IntArg(0);
            if (position < 1 || line.Args.Count < 6)
            {
                ctx.Out.WriteLine("Invalid command! Please check the format:");
                ctx.Out.WriteLine(Usage);
                return;
            }

            var qso = new QsoData(line.Arg(1).ToUpperInvariant(), LogbookViews.NormalizeFrequency(line.Arg(2)),
                                  line.Arg(3).ToUpperInvariant(), line.Arg(4), line.Arg(5), line.RestFrom(6));

            if (await ctx.Logbook.EditQsoAsync(position, qso))
            {
                LogbookViews.RememberQso(ctx, qso);
                ctx.Out.WriteLine("QSO edited successfully!");
            }
            else
            {
                LogbookViews.WriteQsoFailure(ctx, Usage);
            }
        }
    }

    public sealed class DeleteQsoCommand : Command
    {
        public override string Name => "dq";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Delete QSO";
        public override string Usage => "dq [position] [position] [position] (enter 3 times the same position to delete for security reasons)";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            int p1 = line.IntArg(0), p2 = line.IntArg(1), p3 = line.IntArg(2);
            if (p1 < 1 || p1 != p2 || p2 != p3)
            {
                ctx.Out.WriteLine("Invalid command! Please check the format:");
                ctx.Out.WriteLine("Enter 3 times the param position after the dq command (for security reasons!)");
                ctx.Out.WriteLine("dq [position] [position] [position] ");
                return;
            }

            ctx.Out.WriteLine($"Deleting QSO from Logbook at position: {p1}...");
            ctx.Out.WriteLine(await ctx.Logbook.DeleteQsoAsync(p1) ? "QSO deleted successfully!" : "Unable to delete the QSO!");
        }
    }

    public sealed class OpenLogbookCommand : Command
    {
        public override string Name => "lb";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Open Logbook";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Out.WriteLine("Load Logbook...");
            ctx.Out.WriteLine($"Load Logbook = {await ctx.Logbook.GotoLogbookAsync()}");
            await LogbookViews.ShowPageAsync(ctx, ctx.Session.CurrentPage);
        }
    }

    public sealed class QsoCountCommand : Command
    {
        public override string Name => "qc";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "QSO Count";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Out.WriteLine("Get QSOs Count... ");
            ctx.Out.WriteLine($"QSOs Count = {await ctx.Logbook.GetQsoCountAsync()}");
        }
    }

    public sealed class LogbookPagesCommand : Command
    {
        public override string Name => "lp";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Logbook pages";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Out.WriteLine("Get Logbook Pages... ");
            ctx.Out.WriteLine($"Logbook Pages = {await ctx.Logbook.GetLogbookPagesAsync()}");
        }
    }

    public sealed class QsoPerPageCommand : Command
    {
        public override string Name => "qp";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Get/Set QSO for page";
        public override string Usage => "qp [entries] (valid values: 5, 10, 15, 20, 25, 50, 100, 200)";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            int entries = line.IntArg(0);
            if (entries <= 0)
            {
                ctx.Out.WriteLine("Get QSO for page... ");
                ctx.Out.WriteLine($"QSO for page = {await ctx.Logbook.GetEntriesForPageAsync()}");
                return;
            }

            if (!LogbookViews.ValidEntriesPerPage.Contains(entries))
            {
                ctx.Out.WriteLine("Invalid parameter! Enter one of the following values: 5, 10, 15, 20, 25, 50, 100, 200");
                return;
            }

            ctx.Out.WriteLine($"Set QSO for page to {entries}... ");
            int result = await ctx.Logbook.SetEntriesForPageAsync(entries);
            ctx.Out.WriteLine(result == entries ? $"QSO for page = {entries}" : $"Unable to set QSO for page = {entries}");
        }
    }

    public sealed class ViewModeCommand : Command
    {
        public override string Name => "cv";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Get/Set current view mode";
        public override string Usage => "cv [mode] (mode: [raw], [text] OR [adif])";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            string requested = line.Arg(0);
            if (requested.Length == 0)
            {
                ctx.Out.WriteLine($"Current Logbook View Mode is: {ctx.Session.ViewMode.ToKeyword()}");
            }
            else if (!ViewModeExtensions.TryParse(requested, out ViewMode mode))
            {
                ctx.Out.WriteLine("Invalid View Mode! (mode is [raw], [text] or [adif])");
            }
            else
            {
                ctx.Session.ViewMode = mode;
                ctx.Settings.ViewMode = mode.ToKeyword();
                ctx.Out.WriteLine($"Set current Logbook View Mode: {mode.ToKeyword()}");
            }
            return Task.CompletedTask;
        }
    }

    public sealed class GotoPageCommand : Command
    {
        public override string Name => "gp";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Goto Page";
        public override string Usage => "gp [page]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            int page = line.IntArg(0);
            if (page < 1)
            {
                ctx.Out.WriteLine($"Invalid page number! Usage: {Usage}");
                return Task.CompletedTask;
            }
            return LogbookViews.GotoPageAsync(ctx, page);
        }
    }

    public sealed class PageDownCommand : Command
    {
        public override string Name => "pd";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Page Down";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            int page = ctx.Session.CurrentPage > 1 ? ctx.Session.CurrentPage - 1 : 1;
            ctx.Session.CurrentPage = page;
            return LogbookViews.GotoPageAsync(ctx, page);
        }
    }

    public sealed class PageUpCommand : Command
    {
        public override string Name => "pu";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Page Up";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            int page = ctx.Session.CurrentPage + 1;
            ctx.Session.CurrentPage = page;
            return LogbookViews.GotoPageAsync(ctx, page);
        }
    }

    public sealed class CurrentPageCommand : Command
    {
        public override string Name => "cp";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Current Page";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Out.WriteLine("Get Current Logbook Page... ");
            int page = await ctx.Logbook.GetCurrentLogbookPageAsync();
            if (page <= 0)
            {
                ctx.Out.WriteLine("Unable to get Current Logbook Page! Are you logged in?");
                return;
            }

            ctx.Out.WriteLine($"Current Logbook Page = {page}");
            ctx.Session.CurrentPage = page;
        }
    }

    public sealed class OrderDateAscCommand : Command
    {
        public override string Name => "da";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Order Date Asc";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Out.WriteLine("Set Logbook Order Date Asc... ");
            ctx.Out.WriteLine(await ctx.Logbook.SetLogbookDateOrderAsync(descending: false)
                ? "Logbook Order Date is \"Asc\""
                : "Unable to Set Logbook Order Date Asc");
        }
    }

    public sealed class OrderDateDescCommand : Command
    {
        public override string Name => "dd";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Order Date Desc";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Out.WriteLine("Set Logbook Order Date Desc... ");
            ctx.Out.WriteLine(await ctx.Logbook.SetLogbookDateOrderAsync(descending: true)
                ? "Logbook Order Date is \"Desc\""
                : "Unable to Set Logbook Order Date Desc");
        }
    }

    public sealed class PageTextCommand : Command
    {
        public override string Name => "tt";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Get Table Content Text";
        public override string Usage => "tt [page]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line) => LogbookViews.PageTextAsync(ctx, line.IntArg(0));
    }

    public sealed class PageRawCommand : Command
    {
        public override string Name => "tr";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Get Table Content Raw";
        public override string Usage => "tr [page]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line) => LogbookViews.PageRawAsync(ctx, line.IntArg(0));
    }

    public sealed class PageXmlCommand : Command
    {
        public override string Name => "tx";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Get Table Content XML";
        public override string Usage => "tx [page]";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line) => LogbookViews.PageXmlAsync(ctx, line.IntArg(0));
    }

    public sealed class RangeTextCommand : Command
    {
        public override string Name => "qt";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Get QSOs by range Text View";
        public override string Usage => "qt [start position] [end position]";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            IReadOnlyList<LogbookEntry>? entries = await LogbookViews.RangeAsync(ctx, line, "Text View");
            if (entries == null) return;

            if (entries.Count > 0)
            {
                ctx.Out.WriteLine();
                LogbookViews.WriteAll(ctx, LogbookFormatter.RangeTable(entries));
            }
            ctx.Out.WriteLine($"Found {entries.Count} entries");
        }
    }

    public sealed class RangeRawCommand : Command
    {
        public override string Name => "qr";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Get QSOs by range Text Raw";
        public override string Usage => "qr [start position] [end position]";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            IReadOnlyList<LogbookEntry>? entries = await LogbookViews.RangeAsync(ctx, line, "Text Raw");
            if (entries == null) return;

            if (entries.Count > 0)
            {
                LogbookViews.WriteAll(ctx, LogbookFormatter.TabSeparated(entries));
                ctx.Out.WriteLine();
            }
            ctx.Out.WriteLine($"Found {entries.Count} entries");
        }
    }

    public sealed class RangeAdifCommand : Command
    {
        public override string Name => "qa";
        public override string Group => CommandGroups.Logbook;
        public override string Description => "Get QSOs by range ADIF";
        public override string Usage => "qa [start position] [end position]";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            IReadOnlyList<LogbookEntry>? entries = await LogbookViews.RangeAsync(ctx, line, "ADIF");
            if (entries == null) return;

            if (entries.Count > 0)
            {
                ctx.Out.WriteLine();
                LogbookViews.WriteAll(ctx, LogbookFormatter.Adif(entries));
            }
            ctx.Out.WriteLine();
            ctx.Out.WriteLine($"Found {entries.Count} entries");
        }
    }
}
