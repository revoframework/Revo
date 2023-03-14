using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace Revo.Tools.DatabaseMigrator
{
    class Program
    {
        private static ILogger logger;

        static Task<int> Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Default", LogLevel.Information)
                    .AddConsole();
            });

            ILogger logger = loggerFactory.CreateLogger<Program>();

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

            DatabaseMigrator migrator = new DatabaseMigrator(verb, logger);
            await migrator.UpgradeAsync(verb);

            return 0;
        }

        private static async Task<int> Preview(PreviewVerb verb)
        {
            logger.LogInformation("Running command: preview (showing only preview of migrations to be applied)");

            DatabaseMigrator migrator = new DatabaseMigrator(verb, logger);
            await migrator.PreviewAsync(verb);

            return 0;
        }
    }
}
