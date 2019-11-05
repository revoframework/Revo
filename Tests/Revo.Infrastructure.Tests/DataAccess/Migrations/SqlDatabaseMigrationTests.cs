using FluentAssertions;
using NSubstitute;
using Revo.Infrastructure.DataAccess.Migrations;
using Xunit;

namespace Revo.Infrastructure.Tests.DataAccess.Migrations
{
    public class SqlDatabaseMigrationTests
    {
        [Fact]
        public void Checksum_SameForOneSql()
        {
            var sut1 = Substitute.ForPartsOf<SqlDatabaseMigration>();
            sut1.SqlCommands.Returns(new[] { "CREATE TABLE abc (id int);" });
            
            var sut2 = Substitute.ForPartsOf<SqlDatabaseMigration>();
            sut2.SqlCommands.Returns(new[] { "CREATE TABLE abc (id int);" });

            sut1.Checksum.Should().Be(sut2.Checksum);
        }
        
        [Fact]
        public void Checksum_DiffersForTwoSqlCommands()
        {
            var sut1 = Substitute.ForPartsOf<SqlDatabaseMigration>();
            sut1.SqlCommands.Returns(new[] { "CREATE TABLE abc (id int);" });
            
            var sut2 = Substitute.ForPartsOf<SqlDatabaseMigration>();
            sut2.SqlCommands.Returns(new[] { "CREATE TABLE abc2 (id int);" });

            sut1.Checksum.Should().NotBe(sut2.Checksum);
        }
    }
}