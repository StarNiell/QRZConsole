using System;
using System.Collections.Generic;
using System.Linq;
using QRZConsole.Core.Commands.Builtin;
using QRZConsole.Core.Formatting;

namespace QRZConsole.Core.Commands
{
    /// <summary>The set of available commands, looked up by name or alias.</summary>
    public sealed class CommandCatalog
    {
        private readonly List<Command> _commands = new List<Command>();
        private readonly Dictionary<string, Command> _byName = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<Command> Commands => _commands;

        public CommandCatalog Add(Command command)
        {
            foreach (string name in new[] { command.Name }.Concat(command.Aliases))
            {
                if (_byName.ContainsKey(name))
                    throw new InvalidOperationException($"Command name \"{name}\" is registered twice.");
                _byName[name] = command;
            }

            _commands.Add(command);
            return this;
        }

        public Command? Find(string name) => _byName.TryGetValue(name ?? string.Empty, out Command? c) ? c : null;

        /// <summary>Prints the command list grouped as in <see cref="CommandGroups.Ordered"/>.</summary>
        public void WriteCommandList(IConsoleOutput output)
        {
            const int width = 40;
            string line = new string('-', 73);

            output.WriteLine("Command List");
            output.WriteLine(line);
            output.WriteLine(Text.Fixed("  Description", width) + "command [param] ...");
            output.WriteLine(line);

            IEnumerable<string> groups = CommandGroups.Ordered
                .Concat(_commands.Select(c => c.Group).Distinct().Where(g => !CommandGroups.Ordered.Contains(g)));

            foreach (string group in groups)
            {
                List<Command> inGroup = _commands.Where(c => c.Group == group).ToList();
                if (inGroup.Count == 0) continue;

                output.WriteLine($"{group}:");
                foreach (Command c in inGroup)
                    output.WriteLine(Text.Fixed($"  {c.Description}", width) + c.Usage);
            }

            output.WriteLine(line);
        }

        /// <summary>All the built-in commands, in command-list order.</summary>
        public static CommandCatalog CreateDefault()
        {
            return new CommandCatalog()
                // Credentials
                .Add(new IsLoggedCommand())
                .Add(new IsLoggedLastStatusCommand())
                .Add(new LoginCommand())
                .Add(new LogOutCommand())
                // Search
                .Add(new LookupCommand())
                .Add(new CheckWorkedCommand())
                .Add(new LookupAndCheckWorkedCommand())
                .Add(new CheckWorkedAndLookupCommand())
                // Logbook
                .Add(new AddQsoCommand())
                .Add(new EditQsoCommand())
                .Add(new DeleteQsoCommand())
                .Add(new OpenLogbookCommand())
                .Add(new QsoCountCommand())
                .Add(new LogbookPagesCommand())
                .Add(new QsoPerPageCommand())
                .Add(new ViewModeCommand())
                .Add(new GotoPageCommand())
                .Add(new PageDownCommand())
                .Add(new PageUpCommand())
                .Add(new CurrentPageCommand())
                .Add(new OrderDateAscCommand())
                .Add(new OrderDateDescCommand())
                .Add(new PageTextCommand())
                .Add(new PageRawCommand())
                .Add(new PageXmlCommand())
                .Add(new RangeTextCommand())
                .Add(new RangeRawCommand())
                .Add(new RangeAdifCommand())
                // Cluster DX
                .Add(new ClusterOpenCommand())
                .Add(new ClusterShowBandCommand())
                .Add(new ClusterShowFrequencyCommand())
                .Add(new ClusterSearchCallCommand())
                .Add(new ClusterRawCommand())
                .Add(new ClusterSendSpotCommand())
                .Add(new ClusterItemsCommand())
                .Add(new ClusterDebugCommand())
                .Add(new ClusterCloseCommand())
                // General / utility
                .Add(new ClearCommand())
                .Add(new FindDxccCommand())
                .Add(new LoadLocatorsCommand())
                .Add(new LocatorListCommand())
                .Add(new SetFrequencyCommand())
                .Add(new SetQrzCommand())
                .Add(new QrzHomeCommand())
                .Add(new SwitchViewCommand())
                .Add(new SwitchCheckLoggedCommand())
                .Add(new FullScreenCommand())
                .Add(new ThemeCommand())
                .Add(new LastDataCommand())
                .Add(new BrowserDebugCommand())
                .Add(new BrowserFieldsCommand())
                .Add(new CommandListCommand())
                .Add(new ShortcutListCommand())
                .Add(new QuitCommand());
        }
    }
}
