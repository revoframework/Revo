using McMaster.NETCore.Plugins;
using Microsoft.Data.Sqlite;
using Ninject;
using Ninject.Modules;
using Npgsql;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.DataAccess.Migrations.Providers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Revo.Core.Core;
using Revo.Core.Events;

namespace Revo.Tools.DatabaseMigrator
{
    public class DatabaseMigrator : IDisposable
    {
        private readonly StandardKernel kernel = new StandardKernel();
        private readonly IDatabaseMigrationRegistry migrationRegistry = new DatabaseMigrationRegistry();
        private readonly ILogger logger;
        private readonly DatabaseMigrationExecutionOptions executionOptions = new();
        private IDatabaseMigrationExecutor executor;
        private IDatabaseMigrationSelector migrationSelector;
        private AdoNetDatabaseMigrationProvider migrationProvider;
        private IDatabaseMigrationDiscovery[] migrationDiscoveries;

        public DatabaseMigrator(ICommonOptions options, ILogger logger)
        {
            this.Options = options;
            this.logger = logger;
        }

        public ICommonOptions Options { get; }

        public async Task UpgradeAsync(UpgradeVerb verb)
        {
            await InitializeAsync();

            var appliedMigrations = await executor.ExecuteAsync();

            if (appliedMigrations.Count == 0)
            {
                logger.LogInformation("There are no pending migrations. No changes have been made.");
            }
        }

        public async Task PreviewAsync(PreviewVerb verb)
        {
            await InitializeAsync();
            
            var appliedMigrations = await executor.PreviewAsync();

            if (appliedMigrations.Count == 0)
            {
                logger.LogInformation("There are no pending migrations to be applied.");
            }
            else
            {
                string GetMigrationLines(PendingModuleMigration moduleMigration)
                {
                    return string.Join('\n',
                        moduleMigration.Migrations.Select((x, i) =>
                            $"  {i + 1}. {x.ToString(false)}{(x.ModuleName != moduleMigration.Specifier.ModuleName ? " (dependency)" : "")}"));
                }

                var moduleInfos = appliedMigrations.Select(x => $"{x.Specifier.ModuleName}{(x.Migrations.Any(y => y.Version != null) ? $" to version {x.Migrations.OrderByDescending(y => y.Version).First().Version}" : "")} ({x.Migrations.Count()} migration(s)):\n{GetMigrationLines(x)}");
                string moduleInfosLines = string.Join("\n\n", moduleInfos);
                logger.LogInformation($"There are {appliedMigrations.Count} modules to be migrated:\n\n{moduleInfosLines}");
            }
        }

        private async Task InitializeAsync()
        {
            if (executor != null)
            {
                return;
            }

            LoadModules();

            CreateMigrationProvider();

            executionOptions.EnvironmentTags = Options.EnvironmentTags?.ToArray() ?? new string[0];
            executionOptions.MigrateOnlySpecifiedModules = Options.Modules?.Count() > 0
                ? Options.Modules.Select(x =>
                {
                    string[] parts = x.Split('@', 2);
                    if (parts.Length > 1)
                    {
                        return new DatabaseMigrationSearchSpecifier(parts[0], DatabaseVersion.Parse(parts[1]));
                    }
                    else
                    {
                        return new DatabaseMigrationSearchSpecifier(x, null);
                    }
                }).ToArray()
                : null;

            migrationSelector = new DatabaseMigrationSelector(migrationRegistry, migrationProvider,
                executionOptions.MigrationSelectorOptions);

            Regex fileNameRegex = Options.FileNameRegex != null
                ? new Regex(Options.FileNameRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase)
                : null;

            foreach (string path in Options.DirectoryPaths)
            {
                kernel.Bind<FileDatabaseMigrationDiscoveryPath>()
                    .ToConstant(new FileDatabaseMigrationDiscoveryPath(path, true, fileNameRegex));
            }

            if (!Options.DirectoryPaths.Any() && !Options.Assemblies.Any())
            {
                logger.LogError("You have not passed any --directoryPaths nor --assemblies to load the migrations from. No migrations will be performed.");
            }

            migrationDiscoveries = new IDatabaseMigrationDiscovery[]
            {
                    new FileDatabaseMigrationDiscovery(kernel.GetBindings(typeof(FileDatabaseMigrationDiscoveryPath)).Count() > 0
                        ? kernel.GetAll<FileDatabaseMigrationDiscoveryPath>().ToArray() : new FileDatabaseMigrationDiscoveryPath[0]),
                    new ResourceDatabaseMigrationDiscovery(kernel.GetBindings(typeof(ResourceDatabaseMigrationDiscoveryAssembly)).Count() > 0
                        ? kernel.GetAll<ResourceDatabaseMigrationDiscoveryAssembly>().ToArray()
                        : new ResourceDatabaseMigrationDiscoveryAssembly[0])
            };

            executor = new DatabaseMigrationExecutor(new[] { migrationProvider }, migrationRegistry,
                migrationDiscoveries, migrationSelector, executionOptions, logger);
        }

        private void LoadModules()
        {
            var loadedAssemblies = new Dictionary<string, Assembly>();

            foreach (var assemblyName in Options.Assemblies)
            {
                var pluginLoader = PluginLoader.CreateFromAssemblyFile(
                    assemblyFile: assemblyName, new[] { typeof(INinjectModule), typeof(ResourceDatabaseMigrationDiscoveryAssembly) },
                    config =>
                    {
                        config.PreferSharedTypes = true;
                    });

                Assembly mainAssembly = pluginLoader.LoadDefaultAssembly();
                
                loadedAssemblies.Add(mainAssembly.GetName().Name, mainAssembly);

                LoadReferencedAssembliesRecursive(mainAssembly, pluginLoader, loadedAssemblies);
            }

            LoadAssemblyModules(loadedAssemblies.Values);
        }

        private INinjectModule[] GetNinjectModules(Assembly assembly)
        {
            return assembly.IsDynamic
                ? new INinjectModule[0]
                : assembly.ExportedTypes.Where(IsLoadableModule)
                    .Select(type => Activator.CreateInstance(type) as INinjectModule)
                    .ToArray();
        }

        private bool IsLoadableModule(Type type)
        {
            if (!typeof(INinjectModule).IsAssignableFrom(type)
                || type.IsAbstract
                || type.IsInterface
                || type.GetConstructor(Type.EmptyTypes) == null)
            {
                return false;
            }

            return true;
        }

        private void LoadAssemblyModules(IReadOnlyCollection<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                try
                {
                    var modules = GetNinjectModules(assembly).Where(x => !kernel.HasModule(x.Name)).ToArray();
                    if (modules.Length > 0)
                    {
                        kernel.Load(modules);
                    }
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, $"Failed to load referenced assembly");
                    throw;
                }
            }
        }

        private void LoadReferencedAssembliesRecursive(Assembly assembly, PluginLoader pluginLoader,
            Dictionary<string, Assembly> loadedAssemblies)
        {
            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                if (!loadedAssemblies.ContainsKey(assemblyName.Name)
                    && !assemblyName.Name.StartsWith("Microsoft.")
                    && !assemblyName.Name.StartsWith("System."))
                {
                    var referencedAssembly = pluginLoader.LoadAssembly(assemblyName);

                    loadedAssemblies.Add(referencedAssembly.GetName().Name, referencedAssembly);
                    LoadReferencedAssembliesRecursive(referencedAssembly, pluginLoader, loadedAssemblies);
                }
            }
        }

        private void CreateMigrationProvider()
        {
            IDbConnection dbConnection;
            IDatabaseMigrationScripter scripter;

            switch (Options.DatabaseProvider)
            {
                case DatabaseProvider.Npgsql:
                    dbConnection = new NpgsqlConnection(Options.ConnectionString);
                    scripter = new PgsqlMigrationScripter();
                    logger.LogInformation("Using Npgsql database provider with PostgreSQL scripter");
                    break;

                case DatabaseProvider.SqlServer:
                    dbConnection = new SqlConnection(Options.ConnectionString);
                    scripter = new MssqlMigrationScripter();
                    logger.LogInformation("Using SQL Server database provider with MSSQL scripter");
                    break;
                
                case DatabaseProvider.SQLite:
                    dbConnection = new SqliteConnection(Options.ConnectionString);
                    scripter = new SqliteMigrationScripter();
                    logger.LogInformation("Using SQLite file database provider with SQLite scripter");
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Unknown database provider value");
            }

            var serviceLocator = new NinjectServiceLocator(kernel);
            var eventBus = new EventBus(serviceLocator);

            migrationProvider = new AdoNetDatabaseMigrationProvider(dbConnection, scripter, eventBus, logger);
        }

        public void Dispose()
        {
            kernel?.Dispose();
            migrationProvider?.Dispose();
        }
    }
}