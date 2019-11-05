using System.Collections.Generic;
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
                ModuleName = "appModule1"
            });
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule1"
            });
            sut.AddMigration(new FakeDatabaseMigration()
            {
                ModuleName = "appModule2"
            });

            var result = sut.GetAvailableModules();
            result.Should().BeEquivalentTo("appModule1", "appModule2");
        }
    }
}