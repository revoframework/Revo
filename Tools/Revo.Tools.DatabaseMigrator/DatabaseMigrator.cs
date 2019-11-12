using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Ninject;
using Ninject.Modules;
using NLog;
using Npgsql;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.Tools.DatabaseMigrator
{
    public class DatabaseMigrator : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly StandardKernel kernel = new StandardKernel();
        private readonly IDatabaseMigrationRegistry migrationRegistry = new DatabaseMigrationRegistry();
        private readonly DatabaseMigrationExecutionOptions executionOptions = new DatabaseMigrationExecutionOptions();
        private IDatabaseMigrationExecutor executor;
        private IDatabaseMigrationSelector migrationSelector;
        private AdoNetDatabaseMigrationProvider migrationProvider;
        private IDatabaseMigrationDiscovery[] migrationDiscoveries;

        private static HashSet<string> assemblySearchPaths = new HashSet<string>();

        static DatabaseMigrator()
        {
            assemblySearchPaths.Add(Path.GetDirectoryName("."));
            assemblySearchPaths.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromRegisteredPaths);
        }

        public DatabaseMigrator(ICommonOptions options)
        {
            this.Options = options;
        }

        public ICommonOptions Options { get; }

        private static Assembly LoadFromRegisteredPaths(object sender, ResolveEventArgs args)
        {
            foreach (string searchPath in assemblySearchPaths)
            {
                string assemblyPath = Path.Combine(searchPath, new AssemblyName(args.Name).Name + ".dll");
                if (!File.Exists(assemblyPath))
                {
                    continue;
                }

                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                return assembly;
            }

            return null;
        }

        public async Task UpgradeAsync(UpgradeVerb verb)
        {
            await InitializeAsync();

            var appliedMigrations = await executor.ExecuteAsync();

            if (appliedMigrations.Count == 0)
            {
                Logger.Info("There are no pending migrations. No changes have been made.");
            }
        }

        public async Task PreviewAsync(PreviewVerb verb)
        {
            await InitializeAsync();
            
            var appliedMigrations = await executor.PreviewAsync();

            if (appliedMigrations.Count == 0)
            {
                Logger.Info("There are no pending migrations to be applied.");
            }
            else
            {
                string GetMigrationLines(PendingModuleMigration moduleMigration)
                {
                    return string.Join('\n',
                        moduleMigration.Migrations.Select((x, i) =>
                            $"  {i + 1}. {x.ToString(false)}{(x.ModuleName != moduleMigration.Specifier.ModuleName ? " (dependency)" : "")}"));
                }

                var moduleInfos = appliedMigrations.Select(x => $"{x.Specifier.ModuleName} to version {x.Migrations.OrderByDescending(y => y.Version).First().Version} ({x.Migrations.Count()} migration(s)):\n{GetMigrationLines(x)}");
                string moduleInfosLines = string.Join("\n\n", moduleInfos);
                Logger.Info($"There are {appliedMigrations.Count} modules to be migrated:\n\n{moduleInfosLines}");
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
                        return new DatabaseMigrationSpecifier(parts[0], DatabaseVersion.Parse(parts[1]));
                    }
                    else
                    {
                        return new DatabaseMigrationSpecifier(x, null);
                    }
                }).ToArray()
                : null;

            migrationSelector = new DatabaseMigrationSelector(migrationRegistry, migrationProvider);

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
                Logger.Error("You have not passed any --directoryPaths nor --assemblies to load the migrations from. No migrations will be performed.");
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
                migrationDiscoveries, migrationSelector, executionOptions);
        }

        private void LoadModules()
        {
            var loadedAssemblies = new Dictionary<string, Assembly>();

            foreach (var assemblyName in Options.Assemblies)
            {
                var assembly = Assembly.LoadFile(assemblyName);
                loadedAssemblies.Add(assembly.GetName().Name, assembly);

                if (assemblyName.Contains(".dll"))
                {
                    string assemblyPath = Path.GetFullPath(assemblyName);
                    string assemblyDirectory = Path.GetDirectoryName(assemblyPath);
                    assemblySearchPaths.Add(assemblyDirectory);
                }

                LoadReferencedAssembliesRecursive(assembly, loadedAssemblies);
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
                var modules = GetNinjectModules(assembly).Where(x => !kernel.HasModule(x.Name)).ToArray();
                if (modules.Length > 0)
                {
                    kernel.Load(modules);
                }
            }
        }

        private void LoadReferencedAssembliesRecursive(Assembly assembly, Dictionary<string, Assembly> loadedAssemblies)
        {
            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                if (!loadedAssemblies.ContainsKey(assemblyName.Name)
                    && !assemblyName.Name.StartsWith("Microsoft.")
                    && !assemblyName.Name.StartsWith("System."))
                {
                    var referencedAssembly = Assembly.Load(assemblyName);
                    loadedAssemblies.Add(referencedAssembly.GetName().Name, referencedAssembly);
                    LoadReferencedAssembliesRecursive(referencedAssembly, loadedAssemblies);
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
                    Logger.Info("Using Npgsql database provider with PostgreSQL scripter");
                    break;

                case DatabaseProvider.SqlServer:
                    dbConnection = new SqlConnection(Options.ConnectionString);
                    scripter = new MssqlMigrationScripter();
                    Logger.Info("Using SQL Server database provider with MSSQL scripter");
                    break;
                
                case DatabaseProvider.SQLite:
                    dbConnection = new SqliteConnection(Options.ConnectionString);
                    scripter = new SqliteMigrationScripter();
                    Logger.Info("Using SQLite file database provider with SQLite scripter");
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Unknown database provider value");
            }

            migrationProvider = new AdoNetDatabaseMigrationProvider(dbConnection, scripter);
        }

        public void Dispose()
        {
            kernel?.Dispose();
            migrationProvider?.Dispose();
        }
    }
}