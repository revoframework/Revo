using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace Revo.Tools.DatabaseMigrator
{
    class Program
    {
        private static ILoggerFactory loggerFactory;
        private static ILogger<Program> logger;

        static Task<int> Main(string[] args)
        {
            loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Default", LogLevel.Information)
                    .AddConsole();
            });

            logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation($"Revo Database Migrator (v. {Assembly.GetEntryAssembly()?.GetName().Version}) tool running in standalone mode");

            return Parser.Default.ParseArguments<UpgradeVerb, PreviewVerb>(args)
                .MapResult(
                    (UpgradeVerb opts) => Upgrade(opts),
                    (PreviewVerb opts) => Preview(opts),
                    errs => Task.FromResult(1));
        }

        private static async Task<int> Upgrade(UpgradeVerb verb)
        {
            logger.LogInformation("Running command: upgrade");

            DatabaseMigrator migrator = new DatabaseMigrator(verb, loggerFactory.CreateLogger<DatabaseMigrator>(),
                loggerFactory);
            await migrator.UpgradeAsync(verb);

            return 0;
        }

        private static async Task<int> Preview(PreviewVerb verb)
        {
            logger.LogInformation("Running command: preview (showing only preview of migrations to be applied)");

            DatabaseMigrator migrator = new DatabaseMigrator(verb, loggerFactory.CreateLogger<DatabaseMigrator>(),
                loggerFactory);
            await migrator.PreviewAsync(verb);

            return 0;
        }
    }
}
