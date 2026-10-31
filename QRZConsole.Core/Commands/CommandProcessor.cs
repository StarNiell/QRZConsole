using System;
using System.Threading;
using System.Threading.Tasks;

namespace QRZConsole.Core.Commands
{
    /// <summary>
    /// Parses and runs command lines, one at a time: a command that drives the browser
    /// must finish before the next one starts, otherwise two commands would fight over the same page.
    /// </summary>
    public sealed class CommandProcessor
    {
        private readonly CommandContext _context;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

        public CommandProcessor(CommandContext context)
        {
            _context = context;
        }

        /// <summary>True while a command is running.</summary>
        public bool IsBusy => _gate.CurrentCount == 0;

        /// <summary>
        /// Runs a command line. With <paramref name="echo"/> the line is printed first, as when typed.
        /// Errors are reported on the console, never thrown.
        /// </summary>
        public async Task ExecuteAsync(string line, bool echo = true)
        {
            CommandLine parsed = CommandLine.Parse(line);
            if (parsed.IsEmpty) return;

            if (echo)
            {
                _context.Out.WriteLine();
                _context.Out.WriteLine($">{line}");
            }

            Command? command = _context.Catalog.Find(parsed.Name);
            if (command == null)
            {
                _context.Out.WriteLine($"Invalid command: {line}   (enter \"cm\" for the command list)");
                return;
            }

            await _gate.WaitAsync();
            try
            {
                await command.ExecuteAsync(_context, parsed);
            }
            catch (Exception ex)
            {
                _context.Out.WriteLine($"#Error in \"{command.Name}\": {ex.Message}");
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
