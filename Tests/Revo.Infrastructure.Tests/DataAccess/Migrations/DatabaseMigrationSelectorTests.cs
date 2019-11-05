using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Revo.Infrastructure.DataAccess.Migrations;
using Xunit;

namespace Revo.Infrastructure.Tests.DataAccess.Migrations
{
    public class DatabaseMigrationSelectorTests
    {
        private DatabaseMigrationSelector sut;
        private IDatabaseMigrationRegistry migrationRegistry;
        private IDatabaseMigrationProvider migrationProvider;

        public DatabaseMigrationSelectorTests()
        {
            migrationRegistry = Substitute.For<IDatabaseMigrationRegistry>();
            migrationProvider = Substitute.For<IDatabaseMigrationProvider>();

            sut = new DatabaseMigrationSelector(migrationRegistry, migrationProvider);
        }
        
        [Fact]
        public async Task SelectMigrationsAsync()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule2",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1", new string[0], null);
            migrations.Should().ContainInOrder(
                migrationRegistry.Migrations.ElementAt(1),
                migrationRegistry.Migrations.ElementAt(0));
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
                    Version = DatabaseVersion.Parse("1.0.2"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1", new string[0], null);
            migrations.Should().ContainInOrder(
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
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1",
                new string[0], null);
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
                    },
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new []
                    {
                        new [] { "DEV", "CI" },
                        new [] { "CUSTOMER3" }
                    },
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new []
                    {
                        new [] { "DEV", "CI" },
                        new [] { "CUSTOMER1", "CUSTOMER2" }
                    },
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new []
                    {
                        new [] { "PROD" }
                    },
                    Dependencies = new DatabaseMigrationSpecifier[0]
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1",
                new []
                {
                    "CI", "CUSTOMER2"
                }, null);
            migrations.Should().ContainInOrder(
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
                    Version = DatabaseVersion.Parse("1.0.2"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1",
                new string[0], DatabaseVersion.Parse("1.0.1"));
            migrations.Should().ContainInOrder(
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
                    Version = DatabaseVersion.Parse("1.0.2"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                }
            });

            await sut.Awaiting(async x => await sut.SelectMigrationsAsync("appModule1",
                    new string[0], DatabaseVersion.Parse("1.0.1")))
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
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1",
                new string[0], null);
            migrations.Should().ContainInOrder(
                migrationRegistry.Migrations.ElementAt(0),
                migrationRegistry.Migrations.ElementAt(1));
        }

        [Fact]
        public async Task SelectMigrationsAsync_Baseline()
        {
            migrationRegistry.Migrations.Returns(new List<IDatabaseMigration>()
            {
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.2"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Tags = new string[0][],
                    IsBaseline = true,
                    Dependencies = new DatabaseMigrationSpecifier[0]
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1",
                new string[0], null);
            migrations.Should().ContainInOrder(
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
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Tags = new string[0][],
                    IsBaseline = true,
                    Dependencies = new DatabaseMigrationSpecifier[0]
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1",
                new string[0], null);
            migrations.Should().ContainInOrder(
                migrationRegistry.Migrations.ElementAt(0));
        }

        [Fact]
        public async Task SelectMigrationsAsync_Repeatables()
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
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0],
                    IsRepeatable = true
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1",
                new string[0], null);
            migrations.Should().ContainInOrder(
                migrationRegistry.Migrations.ElementAt(1),
                migrationRegistry.Migrations.ElementAt(2));
        }

        [Fact]
        public async Task SelectMigrationsAsync_RepeatablesNotRunWhenVersionsMatch()
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
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0],
                    IsRepeatable = true
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1",
                new string[0], null);
            migrations.Should().BeEmpty();
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
                    Tags = new string[0][],
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", DatabaseVersion.Parse("1.0.1")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", DatabaseVersion.Parse("1.0.0")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1",
                new string[0], null);
            migrations.Should().ContainInOrder(
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
                    Tags = new string[0][],
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", DatabaseVersion.Parse("1.0.1")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "appModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule1", DatabaseVersion.Parse("1.0.0")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Tags = new string[0][],
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule2", DatabaseVersion.Parse("1.0.1")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule2", DatabaseVersion.Parse("1.0.0")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule2",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule2",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1",
                new string[0], null);
            migrations.Should().ContainInOrder(
                migrationRegistry.Migrations.ElementAt(5),
                migrationRegistry.Migrations.ElementAt(3),
                migrationRegistry.Migrations.ElementAt(1),
                migrationRegistry.Migrations.ElementAt(4),
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(0));
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
                    Tags = new string[0][],
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
                    Tags = new string[0][],
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule3", DatabaseVersion.Parse("1.0.0")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule2",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new []
                    {
                        new DatabaseMigrationSpecifier("baseModule3", DatabaseVersion.Parse("1.0.1")),
                    }
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule3",
                    Version = DatabaseVersion.Parse("1.0.1"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule3",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new string[0][],
                    Dependencies = new DatabaseMigrationSpecifier[0]
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1",
                new string[0], null);
            migrations.Should().ContainInOrder(
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
                    },
                    Dependencies = new DatabaseMigrationSpecifier[0]
                },
                new FakeDatabaseMigration()
                {
                    ModuleName = "baseModule1",
                    Version = DatabaseVersion.Parse("1.0.0"),
                    Tags = new []
                    {
                        new [] { "DEV" }
                    },
                    Dependencies = new DatabaseMigrationSpecifier[0]
                }
            });

            var migrations = await sut.SelectMigrationsAsync("appModule1",
                new[] { "DEV" }, null);
            migrations.Should().ContainInOrder(
                migrationRegistry.Migrations.ElementAt(2),
                migrationRegistry.Migrations.ElementAt(0));
        }
    }
}