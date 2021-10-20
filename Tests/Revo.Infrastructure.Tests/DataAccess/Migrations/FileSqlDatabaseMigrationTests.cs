using System.Text.RegularExpressions;
using FluentAssertions;
using Revo.Infrastructure.DataAccess.Migrations;
using Xunit;

namespace Revo.Infrastructure.Tests.DataAccess.Migrations
{
    public class FileSqlDatabaseMigrationTests
    {
        private TestFileSqlDatabaseMigration sut;

        public FileSqlDatabaseMigrationTests()
        {
        }

        [Theory]
        [InlineData(null, "myapp_1.0.1.sql", "myapp", "1.0.1", null, false, false)]
        [InlineData(null, "myapp_baseline_1.0.1.sql", "myapp", "1.0.1", null, true, false)]
        [InlineData(null, "myapp_repeatable_1.0.1.sql", "myapp", "1.0.1", null, false, true)]
        [InlineData(null, "myapp_1.0.1_DEV,CI.sql", "myapp", "1.0.1", "DEV,CI", false, false)]
        public void PropertiesFromFileName(string fileNameRegex, string fileName, string expModuleName, string expVersionString,
            string expTags, bool isBaseline, bool isRepeatable)
        {
            sut = new TestFileSqlDatabaseMigration(fileName, "",
                fileNameRegex != null ? new Regex(fileNameRegex, RegexOptions.IgnoreCase) : null);

            sut.FileName.Should().Be(fileName);
            sut.ModuleName.Should().Be(expModuleName);
            sut.Version.ToString().Should().Be(expVersionString);
            sut.IsBaseline.Should().Be(isBaseline);
            sut.IsRepeatable.Should().Be(isRepeatable);

            if (expTags != null)
            {
                sut.Tags.Should().HaveCount(1);
                sut.Tags[0].Should().Contain(expTags.Split(","));
            }
            else
            {
                sut.Tags.Should().BeEmpty();
            }
        }

        [Fact]
        public void SqlCommands()
        {
            var sql = "CREATE TABLE abc (id int);";
            sut = new TestFileSqlDatabaseMigration("myapp_1.0.1.sql", sql);

            sut.SqlCommands.Should()
                .HaveCount(1)
                .And.Contain(sut.SqlFileContents);
        }
        
        [Fact]
        public void ParsesDescription()
        {
            var sql =
@"
-- Hello
/* Multi
line comment */
-- description: Lorem ipsum dolor sit amet.

CREATE TABLE abc (
    id int
);

DROP TABLE def;
";

            sut = new TestFileSqlDatabaseMigration("myapp_1.0.1.sql", sql);
            sut.Description.Should().Be("Lorem ipsum dolor sit amet.");
        }

        [Fact]
        public void ParsesVersion()
        {
            var sql = 
@"
-- Hello
/* Multi
line comment */
-- version: 1.5.0

CREATE TABLE abc (
    id int
);

DROP TABLE def;
";

            sut = new TestFileSqlDatabaseMigration("myapp.sql", sql);
            sut.Version.Should().Be(DatabaseVersion.Parse("1.5.0"));
        }

        [Fact]
        public void ThrowsIfHeaderVersionDifferentFromFileName()
        {
            var sql = 
@"
-- Hello
/* Multi
line comment */
-- version: 1.6.0

CREATE TABLE abc (
    id int
);

DROP TABLE def;
";


            sut = new TestFileSqlDatabaseMigration("myapp_1.5.0.sql", sql);
        }
        
        [Fact]
        public void ParsesDependencies()
        {
            var sql = 
@"
-- Hello
/* Multi
line comment */
-- dependency: myapp-base@1.2.3
-- dependency: vendor-module

CREATE TABLE abc (
    id int
);

DROP TABLE def;
";

            sut = new TestFileSqlDatabaseMigration("myapp_1.0.1.sql", sql);
            sut.Dependencies.Should()
                .BeEquivalentTo(new[]
                {
                    new DatabaseMigrationSpecifier("myapp-base", DatabaseVersion.Parse("1.2.3")),
                    new DatabaseMigrationSpecifier("vendor-module", null)
                });
        }

        [Fact]
        public void HasDefaultTransactionMode()
        {
            var sql =
                $@"
-- Hello
DROP TABLE def;
";
            sut = new TestFileSqlDatabaseMigration("myapp_1.0.1.sql", sql);
            sut.TransactionMode.Should().Be(DatabaseMigrationTransactionMode.Default);
        }

        [Theory]
        [InlineData("default", DatabaseMigrationTransactionMode.Default)]
        [InlineData("isolated", DatabaseMigrationTransactionMode.Isolated)]
        [InlineData("withoutTransaction", DatabaseMigrationTransactionMode.WithoutTransaction)]
        public void ParsesTransactionModes(string transactionModeString, DatabaseMigrationTransactionMode transactionMode)
        {
            var sql =
                $@"
-- Hello
-- transactionMode: {transactionModeString}
DROP TABLE def;
";
            sut = new TestFileSqlDatabaseMigration("myapp_1.0.1.sql", sql);
            sut.TransactionMode.Should().Be(transactionMode);
        }

        [Fact]
        public void ShouldNotParseAfterContent()
        {
            var sql =
@"
-- Hello
/* Multi
line comment */

CREATE TABLE abc (
    id int
);

-- description: Lorem ipsum dolor sit amet.

DROP TABLE def;
";

            sut = new TestFileSqlDatabaseMigration("myapp_1.0.1.sql", sql);
            sut.Description.Should().BeNull();
        }

        private class TestFileSqlDatabaseMigration : FileSqlDatabaseMigration
        {
            public TestFileSqlDatabaseMigration(string fileName,
                string sqlFileContents, Regex fileNameRegex = null) : base(fileName, fileNameRegex)
            {
                SqlFileContents = sqlFileContents;
            }

            public string SqlFileContents { get; }

            protected override string ReadSqlFileContents()
            {
                return SqlFileContents;
            }
        }
    }
}