using System.Collections.Generic;
using System.Collections.Immutable;
using FluentAssertions;
using Revo.Core.ValueObjects;
using Xunit;

namespace Revo.Core.Tests.ValueObjects
{
    public class SetAsValueTests
    {
        [Fact]
        public void Equals_IsTrueForSameElements()
        {
            IEnumerable<string> x = new[] {"eins", "zwei", "drei"}.ToImmutableHashSet().AsValueObject();
            IEnumerable<string> y = new[] {"eins", "zwei", "drei"}.ToImmutableHashSet().AsValueObject();
            x.Equals(y).Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("eins", "zwei")]
        [InlineData("eins", "zwei", "drei", "vier")]
        public void Equals_IsFalseForDifferentElements(params string[] yElements)
        {
            IEnumerable<string> x = new[] {"eins", "zwei", "drei"}.ToImmutableHashSet().AsValueObject();
            IEnumerable<string> y = (yElements ?? new string[0]).ToImmutableHashSet().AsValueObject();
            x.Equals(y).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_IsTrueForSameElements()
        {
            IEnumerable<string> x = new[] {"eins", "zwei", "drei"}.ToImmutableHashSet().AsValueObject();
            IEnumerable<string> y = new[] {"eins", "zwei", "drei"}.ToImmutableHashSet().AsValueObject();
            x.GetHashCode().Should().Be(y.GetHashCode());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("eins", "zwei")]
        [InlineData("eins", "zwei", "drei", "vier")]
        public void GetHashCode_IsFalseForDifferentElements(params string[] yElements)
        {
            IEnumerable<string> x = new[] {"eins", "zwei", "drei"}.ToImmutableHashSet().AsValueObject();
            IEnumerable<string> y = (yElements ?? new string[0]).ToImmutableHashSet().AsValueObject();
            x.GetHashCode().Should().NotBe(y.GetHashCode());
        }
    }
}
