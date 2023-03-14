using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Revo.Infrastructure.DataAccess.Migrations;
using Xunit;

namespace Revo.Infrastructure.Tests.DataAccess.Migrations
{
    public class DatabaseMigrationExecutorTests
    {
        private DatabaseMigrationExecutor sut;
        private IDatabaseMigrationProvider[] migrationProviders;
        private IDatabaseMigrationRegistry migrationRegistry;
        private IDatabaseMigrationDiscovery[] migrationDiscoveries;
        private IDatabaseMigrationSelector migrationSelector;
        private IDatabaseMigrationExecutionOptions migrationExecutionOptions;

        public DatabaseMigrationExecutorTests()
        {
            migrationProviders = new[]
            {
                Substitute.For<IDatabaseMigrationProvider>()
            };
            migrationProviders[0].GetProviderEnvironmentTags().Returns(new[] { "providerTag" });

            migrationRegistry = Substitute.For<IDatabaseMigrationRegistry>();
            migrationDiscoveries = new[]
            {
                Substitute.For<IDatabaseMigrationDiscovery>()
            };

            migrationSelector = Substitute.For<IDatabaseMigrationSelector>();

            migrationExecutionOptions = Substitute.For<IDatabaseMigrationExecutionOptions>();
            migrationExecutionOptions.EnvironmentTags.Returns(new[] { "optionsTag" });

            sut = new DatabaseMigrationExecutor(migrationProviders,
                migrationRegistry, migrationDiscoveries, migrationSelector,
                migrationExecutionOptions, new NullLogger<DatabaseMigrationExecutor>());
        }

        [Fact]
        public async Task ExecuteAsync_DiscoversAndRegisters()
        {
            var migrations = new[] {Substitute.For<IDatabaseMigration>()};
            migrationDiscoveries[0].DiscoverMigrations().Returns(migrations);

            await sut.ExecuteAsync();

            migrationRegistry.Received(1).AddMigration(migrations[0]);
        }

        [Fact]
        public async Task ExecuteAsync_AppliesSelected()
        {
            var migrations = new[] {Substitute.For<IDatabaseMigration>()};

            migrationRegistry.GetAvailableModules().Returns(new[] {"appModule1"});
            migrationExecutionOptions.MigrateOnlySpecifiedModules.Returns((IReadOnlyCollection<DatabaseMigrationSearchSpecifier>) null);

            migrationSelector.SelectMigrationsAsync(
                Arg.Is<DatabaseMigrationSpecifier[]>(x =>
                    x.SequenceEqual(new[] {new DatabaseMigrationSpecifier("appModule1", null)})),
                Arg.Is<string[]>(x => x.Length == 2 && x.Contains("providerTag") && x.Contains("optionsTag")))
                .Returns(migrations.Select(x => new SelectedModuleMigrations(new DatabaseMigrationSpecifier("appModule1", null), migrations)).ToArray());

            migrationProviders[0].SupportsMigration(migrations[0]).Returns(true);

            await sut.ExecuteAsync();

            migrationProviders[0].Received(1).ApplyMigrationsAsync(migrations);
        }

        [Fact]
        public async Task ExecuteAsync_MatchesModulesByWildcard()
        {
            var migrations = new[] { Substitute.For<IDatabaseMigration>() };

            migrationRegistry.SearchModules("app-*").Returns(new[] { "appmodule-foo" });
            migrationExecutionOptions.MigrateOnlySpecifiedModules.Returns(new[]
            {
                new DatabaseMigrationSearchSpecifier("app-*", null)
            });

            migrationSelector.SelectMigrationsAsync(
                    Arg.Is<DatabaseMigrationSpecifier[]>(x =>
                        x.SequenceEqual(new[] { new DatabaseMigrationSpecifier("appmodule-foo", null) })),
                    Arg.Any<string[]>())
                .Returns(migrations.Select(x => new SelectedModuleMigrations(new DatabaseMigrationSpecifier("appmodule-foo", null), migrations)).ToArray());

            migrationProviders[0].SupportsMigration(migrations[0]).Returns(true);

            await sut.ExecuteAsync();

            migrationProviders[0].Received(1).ApplyMigrationsAsync(migrations);
        }

        [Fact]
        public async Task ExecuteAsync_MatchesModulesByWildcard_NoDuplicateMigrations()
        {
            var migrations = new[] { Substitute.For<IDatabaseMigration>() };

            migrationRegistry.SearchModules("app-*").Returns(new[] { "appmodule-foo" });
            migrationRegistry.SearchModules("*").Returns(new[] { "appmodule-foo" });
            migrationExecutionOptions.MigrateOnlySpecifiedModules.Returns(new[]
            {
                new DatabaseMigrationSearchSpecifier("app-*", null),
                new DatabaseMigrationSearchSpecifier("*", null)
            });

            migrationSelector.SelectMigrationsAsync(
                    Arg.Is<DatabaseMigrationSpecifier[]>(x =>
                        x.SequenceEqual(new[] { new DatabaseMigrationSpecifier("appmodule-foo", null) })),
                    Arg.Any<string[]>())
                .Returns(migrations.Select(x => new SelectedModuleMigrations(new DatabaseMigrationSpecifier("appmodule-foo", null), migrations)).ToArray());

            migrationProviders[0].SupportsMigration(migrations[0]).Returns(true);

            await sut.ExecuteAsync();

            migrationProviders[0].Received(1).ApplyMigrationsAsync(migrations);
        }

        [Fact]
        public async Task ExecuteAsync_ValidatesMigrationsBeforeSelecting()
        {
            var migrations = new[] {Substitute.For<IDatabaseMigration>()};
            
            await sut.ExecuteAsync();

            Received.InOrder(() =>
            {
                migrationRegistry.ValidateMigrations();
                migrationSelector.SelectMigrationsAsync(Arg.Any<DatabaseMigrationSpecifier[]>(), Arg.Any<string[]>());
            });
        }
    }
}