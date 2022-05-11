using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Core;
using Revo.Infrastructure.DataAccess.Migrations;
using Xunit;

namespace Revo.Infrastructure.Tests.DataAccess.Migrations
{
    public class DatabaseMigrationSelectorTests
    {
        private DatabaseMigrationSelector sut;
        private IDatabaseMigrationRegistry migrationRegistry;
        private IDatabaseMigrationProvider migrationProvider;
        private IDatabaseMigrationSelectorOptions selectorOptions;

        public DatabaseMigrationSelectorTests()
        {
            migrationRegistry = Substitute.For<IDatabaseMigrationRegistry>();
            migrationProvider = Substitute.For<IDatabaseMigrationProvider>();
            selectorOptions = Substitute.For<IDatabaseMigrationSelectorOptions>();
            selectorOptions.RerunRepeatableMigrationsOnDependencyUpdate.Returns(true);

            sut = new DatabaseMigrationSelector(migrationRegistry, migrationProvider, selectorOptions);
        }

        [Fact]
        public async Task SelectMigrationsAsync()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule2",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(1),
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_MultipleTopLevelModules()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule2",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[]
                {
                    new DatabaseMigrationSpecifier("appModule1", null),
                    new DatabaseMigrationSpecifier("appModule2", null)
                },
                new string[0]);

            migrations.Should().HaveCount(2);
            migrations.Should().Contain(x => x.Specifier.Equals(new DatabaseMigrationSpecifier("appModule1", null)));
            migrations.Should().Contain(x => x.Specifier.Equals(new DatabaseMigrationSpecifier("appModule2", null)));

            var a1 = migrations.First(x => x.Specifier.ModuleName == "appModule1");
            a1.Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(1),
                migrationRegistry.Migrations.ElementAt(0));

            var a2 = migrations.First(x => x.Specifier.ModuleName == "appModule2");
            a2.Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(2));
        }

        [Fact]
        public async Task SelectMigrationsAsync_TopModuleDependencies()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new [] { new DatabaseMigrationSpecifier("baseModule1", DatabaseVersion.Parse("1.0.0")) }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule2",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new [] { new DatabaseMigrationSpecifier("baseModule1", DatabaseVersion.Parse("1.0.0")) }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[]
                {
                    new DatabaseMigrationSpecifier("appModule1", null),
                    new DatabaseMigrationSpecifier("appModule2", null)
                },
                new string[0]);

            migrations.Should().HaveCount(2);
            migrations.Should().Contain(x => x.Specifier.Equals(new DatabaseMigrationSpecifier("appModule1", null)));
            migrations.Should().Contain(x => x.Specifier.Equals(new DatabaseMigrationSpecifier("appModule2", null)));

            var a1 = migrations.First(x => x.Specifier.ModuleName == "appModule1");
            a1.Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(0));

            var a2 = migrations.First(x => x.Specifier.ModuleName == "appModule2");
            a2.Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(1));
        }

        [Fact]
        public async Task SelectMigrationsAsync_WithHistory()
        {
            migrationProvider.GetMigrationHistoryAsync()
                .Returns(new[]
                {
                    new FakeDatabaseMigrationRecord()
                    {
                        ModuleName = "appModule1",
                        Version = DatabaseVersion.Parse("1.0.1")
                    },
                    new FakeDatabaseMigrationRecord()
                    {
                        ModuleName = "appModule1",
                        Version = DatabaseVersion.Parse("1.0.0")
                    },
                    new FakeDatabaseMigrationRecord()
                    {
                        ModuleName = "appModule2",
                        Version = DatabaseVersion.Parse("1.0.2")
                    }
                });

            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.2")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_NoopWhenVersionsMatch()
        {
            migrationProvider.GetMigrationHistoryAsync()
                .Returns(new[]
                {
                    new FakeDatabaseMigrationRecord()
                    {
                        ModuleName = "appModule1",
                        Version = DatabaseVersion.Parse("1.0.0")
                    }
                });

            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);
            migrations.Should().BeEmpty();
        }

        [Fact]
        public async Task SelectMigrationsAsync_Tagged()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Tags = new []
                    {
                        new [] { "DEV", "CI" },
                        new [] { "CUSTOMER2" }
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new []
                    {
                        new [] { "DEV", "CI" },
                        new [] { "CUSTOMER3" }
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new []
                    {
                        new [] { "DEV", "CI" },
                        new [] { "CUSTOMER1", "CUSTOMER2" }
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new []
                    {
                        new [] { "PROD" }
                    }
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new[]
                {
                    "CI", "CUSTOMER2"
                });

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_TargetVersion()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.2")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", DatabaseVersion.Parse("1.0.1")) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", DatabaseVersion.Parse("1.0.1")));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(1));
        }

        [Fact]
        public async Task SelectMigrationsAsync_NonExistentTargetVersionThrows()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.2")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            await sut.Awaiting(x => sut.SelectMigrationsAsync(
                    new[] { new DatabaseMigrationSpecifier("appModule1", DatabaseVersion.Parse("1.0.1")) },
                    new string[0]))
                .Should()
                .ThrowAsync<DatabaseMigrationException>();
        }
        
        [Fact]
        public async Task SelectMigrationsAsync_AreInOrder()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(0),
                migrationRegistry.Migrations.ElementAt(1));
        }

        [Fact]
        public async Task SelectMigrationsAsync_MigrationNeedNotExistIfAlreadyUpToDate()
        {
            migrationProvider.GetMigrationHistoryAsync()
                .Returns(new[]
                {
                    new FakeDatabaseMigrationRecord()
                    {
                        ModuleName = "appModule1",
                        Version = DatabaseVersion.Parse("1.0.0")
                    }
                });

            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", DatabaseVersion.Parse("1.0.0")) },
                new string[0]);

            migrations.Should().BeEmpty();
        }
        
        [Fact]
        public async Task SelectMigrationsAsync_MigrationNeedNotExistIfAlreadyUpToDate_WithBaseline()
        {
            migrationProvider.GetMigrationHistoryAsync()
                .Returns(new[]
                {
                    new FakeDatabaseMigrationRecord()
                    {
                        ModuleName = "appModule1",
                        Version = DatabaseVersion.Parse("1.0.0")
                    }
                });

            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    IsBaseline = true
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", DatabaseVersion.Parse("1.0.0")) },
                new string[0]);

            migrations.Should().BeEmpty();
        }

        [Fact]
        public async Task SelectMigrationsAsync_Baseline()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.2")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    IsBaseline = true
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(3),
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_BaselineNotUsedOnExistingDBs()
        {
            migrationProvider.GetMigrationHistoryAsync()
                .Returns(new[]
                {
                    new FakeDatabaseMigrationRecord()
                    {
                        ModuleName = "appModule1",
                        Version = DatabaseVersion.Parse("1.0.0")
                    }
                });

            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    IsBaseline = true
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(1));
        }

        [Fact]
        public async Task SelectMigrationsAsync_BaselineThrowsWhenLatestVersionHasOnlyBaselineMigrationPath()
        {
            migrationProvider.GetMigrationHistoryAsync()
                .Returns(new[]
                {
                    new FakeDatabaseMigrationRecord()
                    {
                        ModuleName = "appModule1",
                        Version = DatabaseVersion.Parse("1.0.0")
                    }
                });

            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    IsBaseline = true
                }
            });

            await sut.Awaiting(x => sut.SelectMigrationsAsync(
                    new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                    new string[0]))
                .Should()
                .ThrowAsync<DatabaseMigrationException>();
        }

        [Fact]
        public async Task SelectMigrationsAsync_RepeatablesAppliedFirstTime()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    IsRepeatable = true,
                    Checksum = "xyz"
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_RepeatablesNotRunWhenChecksumsMatch()
        {
            migrationProvider.GetMigrationHistoryAsync()
                .Returns(new[]
                {
                    new FakeDatabaseMigrationRecord()
                    {
                        ModuleName = "appModule1",
                        Checksum = "xyz",
                        TimeApplied = Clock.Current.UtcNow
                    }
                });

            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    IsRepeatable = true,
                    Checksum = "xyz"
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);
            migrations.Should().BeEmpty();
        }

        [Fact]
        public async Task SelectMigrationsAsync_RepeatablesRerunWhenChecksumsNotMatch()
        {
            migrationProvider.GetMigrationHistoryAsync()
                .Returns(new[]
                {
                    new FakeDatabaseMigrationRecord()
                    {
                        ModuleName = "appModule1",
                        Checksum = "abc",
                        TimeApplied = Clock.Current.UtcNow
                    }
                });

            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    IsRepeatable = true,
                    Checksum = "xyz"
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_RepeatablesRerunWhenDependenciesUpdated()
        {
            migrationProvider.GetMigrationHistoryAsync()
                .Returns(new[]
                {
                    new FakeDatabaseMigrationRecord()
                    {
                        ModuleName = "view",
                        Checksum = "xyz",
                        TimeApplied = Clock.Current.UtcNow
                    }
                });

            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "data",
                    Version = DatabaseVersion.Parse("1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "view",
                    IsRepeatable = true,
                    Checksum = "xyz",
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("data", null),
                    }
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[]
                {
                    new DatabaseMigrationSpecifier("data", null),
                    new DatabaseMigrationSpecifier("view", null)
                },
                new string[0]);

            migrations.Should().HaveCount(2);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("data", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(0));
            migrations.ElementAt(1).Specifier.Should().Be(new DatabaseMigrationSpecifier("view", null));
            migrations.ElementAt(1).Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(1));
        }

        [Fact]
        public async Task SelectMigrationsAsync_RepeatablesRunLast()
        {
            migrationProvider.GetMigrationHistoryAsync()
                .Returns(new IDatabaseMigrationRecord[]
                {
                });

            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "a",
                    Version = DatabaseVersion.Parse("1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "b",
                    IsRepeatable = true,
                    Checksum = "xyz"
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "c",
                    Version = DatabaseVersion.Parse("1")
                },
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[]
                {
                    new DatabaseMigrationSpecifier("a", null),
                    new DatabaseMigrationSpecifier("b", null),
                    new DatabaseMigrationSpecifier("c", null)
                },
                new string[0]);

            migrations.Should().HaveCount(3);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("a", null));
            migrations.ElementAt(1).Specifier.Should().Be(new DatabaseMigrationSpecifier("c", null));
            migrations.ElementAt(2).Specifier.Should().Be(new DatabaseMigrationSpecifier("b", null));
        }

        [Fact]
        public async Task SelectMigrationsAsync_RepeatablesNotIfPreviouslyNotRunWhenDependenciesUpdated()
        {
            migrationProvider.GetMigrationHistoryAsync()
                .Returns(new IDatabaseMigrationRecord[]
                {
                });

            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "data",
                    Version = DatabaseVersion.Parse("1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "view",
                    IsRepeatable = true,
                    Checksum = "xyz",
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("data", null),
                    }
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("data", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("data", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_RepeatablesOnlyRunOnce()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Dependencies = new[] { new DatabaseMigrationSpecifier("baseModule1", null) },
                    Version = DatabaseVersion.Parse("1.0.0")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Dependencies = new[] { new DatabaseMigrationSpecifier("baseModule1", null) },
                    Version = DatabaseVersion.Parse("1.0.1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    IsRepeatable = true,
                    Checksum = "xyz"
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(0),
                migrationRegistry.Migrations.ElementAt(1));
        }

        [Fact]
        public async Task SelectMigrationsAsync_LevelOneDependencies()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", DatabaseVersion.Parse("1.0.1")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", DatabaseVersion.Parse("1.0.0")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(3),
                migrationRegistry.Migrations.ElementAt(1),
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_LevelTwoDependencies()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", DatabaseVersion.Parse("1.0.1")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", DatabaseVersion.Parse("1.0.0")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule2", DatabaseVersion.Parse("1.0.1")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule2", DatabaseVersion.Parse("1.0.0")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule2",
                    Version = DatabaseVersion.Parse("1.0.1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule2",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(5),
                migrationRegistry.Migrations.ElementAt(3),
                migrationRegistry.Migrations.ElementAt(1),
                migrationRegistry.Migrations.ElementAt(4),
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_MigrationsToCurrentDependencyVersionsNeedNotExist()
        {
            migrationProvider.GetMigrationHistoryAsync()
                .Returns(new[]
                {
                    new FakeDatabaseMigrationRecord()
                    {
                        ModuleName = "baseModule1",
                        Version = DatabaseVersion.Parse("1.0.0"),
                    }
                });

            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", DatabaseVersion.Parse("1.0.0")),
                    }
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_DependenciesToLatestVersions()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", null),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(1),
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_DependenciesToLatestVersionsFromMultiple()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", null),
                        new DatabaseMigrationSpecifier("baseModule2", null)
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []{ new DatabaseMigrationSpecifier("baseModule3", null) }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule2",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []{ new DatabaseMigrationSpecifier("baseModule3", null) }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule3",
                    Version = DatabaseVersion.Parse("1.0.1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule3",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(4),
                migrationRegistry.Migrations.ElementAt(3),
                migrationRegistry.Migrations.ElementAt(1),
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_OverlappingDependenciesToLatestVersions()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", null),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule2",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", null),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[]
                {
                    new DatabaseMigrationSpecifier("appModule1", null),
                    new DatabaseMigrationSpecifier("appModule2", null)
                },
                new string[0]);

            migrations.Should().HaveCount(2);

            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(3),
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(0));

            migrations.ElementAt(1).Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule2", null));
            migrations.ElementAt(1).Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(1));
        }

        [Fact]
        public async Task SelectMigrationsAsync_OverlappingDependenciesToLatestVersionsWithBaseline()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", null),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule2",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", null),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    IsBaseline = true
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[]
                {
                    new DatabaseMigrationSpecifier("appModule1", null),
                    new DatabaseMigrationSpecifier("appModule2", null)
                },
                new string[0]);

            migrations.Should().HaveCount(2);

            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(0));

            migrations.ElementAt(1).Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule2", null));
            migrations.ElementAt(1).Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(1));
        }

        [Fact]
        public async Task SelectMigrationsAsync_OverlappingLevelTwoDependencies()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", DatabaseVersion.Parse("1.0.0")),
                        new DatabaseMigrationSpecifier("baseModule2", DatabaseVersion.Parse("1.0.0")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule3", DatabaseVersion.Parse("1.0.0")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule2",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule3", DatabaseVersion.Parse("1.0.1")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule3",
                    Version = DatabaseVersion.Parse("1.0.1")
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule3",
                    Version = DatabaseVersion.Parse("1.0.0")
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new string[0]);

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(4),
                migrationRegistry.Migrations.ElementAt(1),
                migrationRegistry.Migrations.ElementAt(3),
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_DependencyTags()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new []
                    {
                        new [] { "DEV" }
                    },
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", DatabaseVersion.Parse("1.0.0"))
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new []
                    {
                        new [] { "PROD" }
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new []
                    {
                        new [] { "DEV" }
                    }
                }
            });

            var migrations = await sut.SelectMigrationsAsync(
                new[] { new DatabaseMigrationSpecifier("appModule1", null) },
                new[] { "DEV" });

            migrations.Should().HaveCount(1);
            migrations.First().Specifier.Should().Be(new DatabaseMigrationSpecifier("appModule1", null));
            migrations.First().Migrations.Should().Equal(
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(0));
        }
    }
}