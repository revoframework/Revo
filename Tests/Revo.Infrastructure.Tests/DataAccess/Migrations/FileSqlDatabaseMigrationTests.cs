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
            sut = new TestFileSqlDatabaseMigration(fileName,
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
            sut = new TestFileSqlDatabaseMigration("myapp_1.0.1.sql", null);
            sut.SqlFileContents = "CREATE TABLE abc (id int);";

            sut.SqlCommands.Should()
                .HaveCount(1)
                .And.Contain(sut.SqlFileContents);
        }
        
        [Fact]
        public void ParsesDescription()
        {
            sut = new TestFileSqlDatabaseMigration("myapp_1.0.1.sql", null);
            sut.SqlFileContents = 
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

            sut.Description.Should().Be("Lorem ipsum dolor sit amet.");
        }
        
        [Fact]
        public void ParsesDependencies()
        {
            sut = new TestFileSqlDatabaseMigration("myapp_1.0.1.sql", null);
            sut.SqlFileContents = 
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

            sut.Dependencies.Should()
                .BeEquivalentTo(
                    new DatabaseMigrationSpecifier("myapp-base", DatabaseVersion.Parse("1.2.3")),
                    new DatabaseMigrationSpecifier("vendor-module", null));
        }
        
        [Fact]
        public void ShouldNotParseAfterContent()
        {
            sut = new TestFileSqlDatabaseMigration("myapp_1.0.1.sql", null);
            sut.SqlFileContents =
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

            sut.Description.Should().BeNull();
        }

        private class TestFileSqlDatabaseMigration : FileSqlDatabaseMigration
        {
            public TestFileSqlDatabaseMigration(string fileName, Regex fileNameRegex = null) : base(fileName, fileNameRegex)
            {
            }

            public string SqlFileContents { get; set; }

            protected override string ReadSqlFileContents()
            {
                return SqlFileContents;
            }
        }
    }
}