using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using McMaster.NETCore.Plugins;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Npgsql;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.Tools.DatabaseMigrator;

public class DatabaseMigrator : IDisposable
{
    private readonly IDatabaseMigrationRegistry migrationRegistry = new DatabaseMigrationRegistry();
    private readonly ILogger<DatabaseMigrator> logger;
    private readonly ILoggerFactory loggerFactory;
    private readonly DatabaseMigrationExecutionOptions executionOptions = new();
    private readonly List<FileDatabaseMigrationDiscoveryPath> fileDiscoveryPaths = [];
    private readonly DatabaseMigratorBindings migratorBindings = new();
    private IDatabaseMigrationExecutor executor;
    private IDatabaseMigrationSelector migrationSelector;
    private AdoNetDatabaseMigrationProvider migrationProvider;
    private IDatabaseMigrationDiscovery[] migrationDiscoveries;

    public DatabaseMigrator(ICommonOptions options, ILogger<DatabaseMigrator> logger, ILoggerFactory loggerFactory)
    {
        Options = options;
        this.logger = logger;
        this.loggerFactory = loggerFactory;
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

            var moduleInfos = appliedMigrations.Select(x =>
                $"{x.Specifier.ModuleName}{(x.Migrations.Any(y => y.Version != null) ? $" to version {x.Migrations.OrderByDescending(y => y.Version).First().Version}" : "")} ({x.Migrations.Count()} migration(s)):\n{GetMigrationLines(x)}");
            string moduleInfosLines = string.Join("\n\n", moduleInfos);
            logger.LogInformation($"There are {appliedMigrations.Count} modules to be migrated:\n\n{moduleInfosLines}");
        }
    }

    private Task InitializeAsync()
    {
        if (executor != null)
        {
            return Task.CompletedTask;
        }

        LoadModules();

        CreateMigrationProvider();

        executionOptions.EnvironmentTags = Options.EnvironmentTags?.ToArray() ?? [];
        executionOptions.MigrateOnlySpecifiedModules = Options.Modules?.Count() > 0
            ? Options.Modules.Select(x =>
            {
                string[] parts = x.Split('@', 2);
                if (parts.Length > 1)
                {
                    return new DatabaseMigrationSearchSpecifier(parts[0], DatabaseVersion.Parse(parts[1]));
                }

                return new DatabaseMigrationSearchSpecifier(x, null);
            }).ToArray()
            : null;

        migrationSelector = new DatabaseMigrationSelector(migrationRegistry,
            executionOptions.MigrationSelectorOptions);

        Regex fileNameRegex = Options.FileNameRegex != null
            ? new Regex(Options.FileNameRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase)
            : null;

        foreach (string path in Options.DirectoryPaths)
        {
            fileDiscoveryPaths.Add(new FileDatabaseMigrationDiscoveryPath(path, true, fileNameRegex));
        }

        if (!Options.DirectoryPaths.Any() && !Options.Assemblies.Any())
        {
            logger.LogError(
                "You have not passed any --directoryPaths nor --assemblies to load the migrations from. No migrations will be performed.");
        }

        var resourceAssemblies = migratorBindings.GetResourceAssemblies();
        migrationDiscoveries =
        [
            new FileDatabaseMigrationDiscovery(fileDiscoveryPaths),
            new ResourceDatabaseMigrationDiscovery(resourceAssemblies.Length > 0
                ? resourceAssemblies
                : [])
        ];

        executor = new DatabaseMigrationExecutor([migrationProvider], migrationRegistry,
            migrationDiscoveries, migrationSelector, executionOptions,
            loggerFactory.CreateLogger<DatabaseMigrationExecutor>());

        return Task.CompletedTask;
    }

    private void LoadModules()
    {
        var loadedAssemblies = new Dictionary<string, Assembly>();

        foreach (var assemblyName in Options.Assemblies)
        {
            var pluginLoader = PluginLoader.CreateFromAssemblyFile(
                assemblyFile: assemblyName,
                [typeof(IDatabaseMigratorPlugin), typeof(ResourceDatabaseMigrationDiscoveryAssembly)],
                config => { config.PreferSharedTypes = true; });

            Assembly mainAssembly = pluginLoader.LoadDefaultAssembly();

            loadedAssemblies.Add(mainAssembly.GetName().Name, mainAssembly);

            LoadReferencedAssembliesRecursive(mainAssembly, pluginLoader, loadedAssemblies);
        }

        ApplyDatabaseMigratorPlugins(loadedAssemblies.Values);
    }

    private static IEnumerable<Type> GetDatabaseMigratorPluginTypes(Assembly assembly)
    {
        if (assembly.IsDynamic)
        {
            yield break;
        }

        Type pluginInterface = typeof(IDatabaseMigratorPlugin);
        foreach (Type type in assembly.ExportedTypes)
        {
            if (!pluginInterface.IsAssignableFrom(type)
                || type.IsAbstract
                || type.IsInterface
                || type.GetConstructor(Type.EmptyTypes) == null)
            {
                continue;
            }

            yield return type;
        }
    }

    private void ApplyDatabaseMigratorPlugins(IEnumerable<Assembly> assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            try
            {
                foreach (Type pluginType in GetDatabaseMigratorPluginTypes(assembly))
                {
                    var plugin = (IDatabaseMigratorPlugin)Activator.CreateInstance(pluginType);
                    plugin.Configure(migratorBindings);
                }
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to load referenced assembly");
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

        migrationProvider = new AdoNetDatabaseMigrationProvider(dbConnection, scripter, new NullEventBus(),
            loggerFactory.CreateLogger<AdoNetDatabaseMigrationProvider>());
    }

    public void Dispose()
    {
        migrationProvider?.Dispose();
    }
}
