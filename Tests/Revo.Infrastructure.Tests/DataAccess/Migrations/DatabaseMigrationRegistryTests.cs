using FluentAssertions;
using Revo.Infrastructure.DataAccess.Migrations;
using Xunit;

namespace Revo.Infrastructure.Tests.DataAccess.Migrations
{
    public class DatabaseMigrationRegistryTests
    {
        private DatabaseMigrationRegistry sut;

        public DatabaseMigrationRegistryTests()
        {
            sut = new DatabaseMigrationRegistry();
        }

        [Fact]
        public void AddMigration()
        {
            var migration = new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.0")
            };

            sut.AddMigration(migration);
            sut.Migrations.Should().Contain(migration);
        }

        [Fact]
        public void GetAvailableModules()
        {
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.0")
            });
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.1")
            });
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule2",
                Version = DatabaseVersion.Parse("1.0.0")
            });

            var result = sut.GetAvailableModules();
            result.Should().BeEquivalentTo("appModule1", "appModule2");
        }

        [Fact]
        public void SearchModules_AsteriskWildcard()
        {
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "app-base",
                Version = DatabaseVersion.Parse("1.0.0")
            });
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "app-base",
                Version = DatabaseVersion.Parse("1.0.1")
            });
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "app-module1",
                Version = DatabaseVersion.Parse("1.0.0")
            });
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "foo-another",
                Version = DatabaseVersion.Parse("1.0.0")
            });

            var result = sut.SearchModules("app-*");
            result.Should().BeEquivalentTo("app-base", "app-module1");
        }

        [Fact]
        public void SearchModules_QuestionMarkWildcard()
        {
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "app-base",
                Version = DatabaseVersion.Parse("1.0.0")
            });
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "app-base",
                Version = DatabaseVersion.Parse("1.0.1")
            });
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "app-module1",
                Version = DatabaseVersion.Parse("1.0.0")
            });
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "foo-another",
                Version = DatabaseVersion.Parse("1.0.0")
            });

            var result = sut.SearchModules("ap?-base");
            result.Should().BeEquivalentTo("app-base");
        }

        [Fact]
        public void Validate_NotThrows()
        {
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.0")
            });
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.1")
            });
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule2",
                Version = DatabaseVersion.Parse("1.0.0")
            });
            
            sut.Invoking(x => x.ValidateMigrations())
                .Should().NotThrow<DatabaseMigrationException>();
        }

        [Fact]
        public void Validate_VersionedRepeatableThrows()
        {
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.0"),
                IsRepeatable = true
            });

            sut.Invoking(x => x.ValidateMigrations())
                .Should().Throw<DatabaseMigrationException>();
        }

        [Fact]
        public void Validate_MixingRepeatableAndNonRepeatable()
        {
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                IsRepeatable = true
            });

            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.0")
            });

            sut.Invoking(x => x.ValidateMigrations())
                .Should().Throw<DatabaseMigrationException>();
        }

        [Fact]
        public void Validate_RepeatableBaselineThrows()
        {
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                IsRepeatable = true,
                IsBaseline = true
            });

            sut.Invoking(x => x.ValidateMigrations())
                .Should().Throw<DatabaseMigrationException>();
        }

        [Fact]
        public void Validate_MultipleRepeatablesThrows()
        {
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                IsRepeatable = true
            });

            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                IsRepeatable = true
            });

            sut.Invoking(x => x.ValidateMigrations())
                .Should().Throw<DatabaseMigrationException>();
        }

        [Fact]
        public void Validate_MultipleRepeatablesWithDifferentTagsNotThrows()
        {
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                IsRepeatable = true,
                Tags = new[] { new[] { "xyz" } }
            });

            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                IsRepeatable = true,
                Tags = new[] { new[] { "abc" } }
            });

            sut.Invoking(x => x.ValidateMigrations())
                .Should().NotThrow<DatabaseMigrationException>();
        }

        [Fact]
        public void Validate_MultipleBaselinesThrows()
        {
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.0"),
                IsBaseline = true
            });

            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.1"),
                IsBaseline = true
            });

            sut.Invoking(x => x.ValidateMigrations())
                .Should().Throw<DatabaseMigrationException>();
        }

        [Fact]
        public void Validate_MultipleBaselinesDifferentTagsNotThrows()
        {
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.0"),
                IsBaseline = true,
                Tags = new [] { new [] { "xyz" } }
            });

            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.1"),
                IsBaseline = true,
                Tags = new[] { new[] { "abc" } }
            });

            sut.Invoking(x => x.ValidateMigrations())
                .Should().NotThrow<DatabaseMigrationException>();
        }

        [Fact]
        public void Validate_VersionDuplicatesThrows()
        {
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.0"),
                Tags = new []{ new[] { "ABC" } }
            });

            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.0"),
                Tags = new[] { new[] { "ABC" } }
            });

            sut.Invoking(x => x.ValidateMigrations())
                .Should().Throw<DatabaseMigrationException>();
        }

        [Fact]
        public void Validate_VersionDuplicatesWithDifferentTagsNotThrows()
        {
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.0"),
                Tags = new []{ new[] { "ABC" } }
            });

            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1",
                Version = DatabaseVersion.Parse("1.0.0"),
                Tags = new[] { new[] { "XYZ" } }
            });

            sut.Invoking(x => x.ValidateMigrations())
                .Should().NotThrow<DatabaseMigrationException>();
        }
    }
}