using System.Threading.Tasks;

namespace QRZConsole.Core.Commands.Builtin
{
    public sealed class IsLoggedCommand : Command
    {
        public override string Name => "il";
        public override string Group => CommandGroups.Credentials;
        public override string Description => "Is Logged";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            ctx.Out.WriteLine("Start IsLogged request...");
            bool logged = await ctx.Logbook.IsLoggedAsync();
            ctx.Out.WriteLine(logged ? $"IsLogged = True by [{ctx.Session.Username}]" : "IsLogged = False");
            ctx.Settings.IsLoggedIn = logged;
        }
    }

    public sealed class IsLoggedLastStatusCommand : Command
    {
        public override string Name => "ls";
        public override string Group => CommandGroups.Credentials;
        public override string Description => "Is Logged (last status)";

        public override Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            bool logged = ctx.Settings.IsLoggedIn;
            ctx.Out.WriteLine(logged
                ? $"IsLogged (last status) = True by [{ctx.Session.Username}]"
                : "IsLogged (last status) = False");
            return Task.CompletedTask;
        }
    }

    public sealed class LoginCommand : Command
    {
        public override string Name => "li";
        public override string Group => CommandGroups.Credentials;
        public override string Description => "Login";
        public override string Usage => "li [username] [password]";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            if (ctx.Settings.IsLoggedIn)
            {
                ctx.Out.WriteLine($"Warning! IsLogged (last status) = True by [{ctx.Settings.Username}]");
                ctx.Out.WriteLine("Before a new Login, check the status with the il command");
                return;
            }

            string user = line.Arg(0).Length > 0 ? line.Arg(0) : ctx.Session.Username;
            string password = line.Arg(1).Length > 0 ? line.Arg(1) : ctx.Session.Password;
            user = user.ToUpperInvariant();

            if (user.Length == 0 || password.Length == 0)
            {
                ctx.Out.WriteLine("Unable to Login: username and password are required fields!");
                return;
            }

            ctx.Session.Username = user;
            ctx.Session.Password = password;
            ctx.Settings.Username = user;
            ctx.Settings.Password = password;
            ctx.Logbook.Qrz = user;
            ctx.Logbook.Password = password;

            ctx.Out.WriteLine($"Login... QRZ=[{user}] - Password=[{new string('*', password.Length)}]");

            if (await ctx.Logbook.LoginAsync())
            {
                ctx.Settings.IsLoggedIn = true;
                ctx.Session.Locators = ctx.Settings.GetLocators(user);
                ctx.Out.WriteLine($"Logged in QRZ=[{user}]");
            }
            else
            {
                ctx.Out.WriteLine(ctx.Logbook.ErrorMessage.Length > 0 ? ctx.Logbook.ErrorMessage : "Login failed!");
            }
        }
    }

    public sealed class LogOutCommand : Command
    {
        public override string Name => "lo";
        public override string Group => CommandGroups.Credentials;
        public override string Description => "LogOut";

        public override async Task ExecuteAsync(CommandContext ctx, CommandLine line)
        {
            if (!ctx.Settings.IsLoggedIn)
            {
                ctx.Out.WriteLine("Warning! IsLogged (last status) = False");
                ctx.Out.WriteLine("Before Logout, check the status with the il command");
                return;
            }

            ctx.Out.WriteLine("Start Log Out...");
            bool done = await ctx.Logbook.LogOutAsync();
            ctx.Out.WriteLine($"Logout = {done}");
            if (!done) return;

            ctx.Settings.IsLoggedIn = false;
            ctx.Settings.Username = string.Empty;
            ctx.Settings.Password = string.Empty;
            ctx.Settings.LastLookup = string.Empty;

            ctx.Session.Username = string.Empty;
            ctx.Session.Password = string.Empty;
            ctx.Session.CurrentQrz = string.Empty;
            ctx.Session.CurrentPage = 1;

            ctx.Logbook.Qrz = string.Empty;
            ctx.Logbook.Password = string.Empty;
        }
    }
}
