using FluentAssertions;
using Revo.Infrastructure.DataAccess.Migrations;
using Xunit;

namespace Revo.Infrastructure.Tests.DataAccess.Migrations
{
    public class DatabaseVersionTests
    {
        [Theory]
        [InlineData(".")]
        [InlineData(".1")]
        [InlineData("1.")]
        [InlineData("1..2")]
        public void Parse_RejectsEmptyVersionParts(string value)
        {
            var act = () => DatabaseVersion.Parse(value);

            act.Should().Throw<System.FormatException>();
        }

        [Fact]
        public void Parse_RoundTripsEmptyVersion()
        {
            var version = new DatabaseVersion();

            DatabaseVersion.Parse(version.ToString()).Fractions.Should().BeEmpty();
        }

        [Fact]
        public void CompareTo_DoesNotOverflowForSignedFractions()
        {
            var positive = new DatabaseVersion(int.MaxValue);
            var negative = new DatabaseVersion(-1);

            positive.CompareTo(negative).Should().BePositive();
            negative.CompareTo(positive).Should().BeNegative();
        }
    }
}
