using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Revo.Tools.DatabaseMigrator
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        static Task<int> Main(string[] args)
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget("target1")
            {
                Layout = @"${message} ${exception}"
            };
            config.AddTarget(consoleTarget);

            var fileTarget = new FileTarget("target2")
            {
                FileName = "${basedir}/migration.log",
                Layout = "${longdate} ${level} ${message} ${exception}"
            };
            config.AddTarget(fileTarget);

            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;

            Logger.Info($"Revo Database Migrator (v. {Assembly.GetEntryAssembly()?.GetName().Version}) tool running in standalone mode");

            return CommandLine.Parser.Default.ParseArguments<UpgradeVerb, PreviewVerb>(args)
                .MapResult(
                    (UpgradeVerb opts) => Upgrade(opts),
                    (PreviewVerb opts) => Preview(opts),
                    errs => Task.FromResult(1));
        }

        private static async Task<int> Upgrade(UpgradeVerb verb)
        {
            Logger.Info("Running command: upgrade");

            DatabaseMigrator migrator = new DatabaseMigrator(verb);
            await migrator.UpgradeAsync(verb);

            return 0;
        }

        private static async Task<int> Preview(PreviewVerb verb)
        {
            Logger.Info("Running command: preview (showing only preview of migrations to be applied)");

            DatabaseMigrator migrator = new DatabaseMigrator(verb);
            await migrator.PreviewAsync(verb);

            return 0;
        }
    }
}
