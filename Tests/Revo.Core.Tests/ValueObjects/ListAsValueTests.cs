using System.Collections.Generic;
using System.Collections.Immutable;
using FluentAssertions;
using Revo.Core.ValueObjects;
using Xunit;

namespace Revo.Core.Tests.ValueObjects
{
    public class ListAsValueTests
    {
        [Fact]
        public void Equals_IsTrueForSameElements()
        {
            IEnumerable<string> x = new[] {"eins", "zwei", null, "drei"}.ToImmutableList().AsValueObject();
            IEnumerable<string> y = new[] {"eins", "zwei", null, "drei"}.ToImmutableList().AsValueObject();
            x.Equals(y).Should().BeTrue();
        }

        [Fact]
        public void Equals_IsFalseForDifferentElements()
        {
            IEnumerable<string> x = new[] {"eins", "zwei", "drei"}.ToImmutableList().AsValueObject();
            IEnumerable<string> y = new[] {"eins", "zwei", "vier"}.ToImmutableList().AsValueObject();
            x.Equals(y).Should().BeFalse();
        }

        [Fact]
        public void Equals_IsFalseForDifferentOrder()
        {
            IEnumerable<string> x = new[] {"eins", "zwei", "drei"}.ToImmutableList().AsValueObject();
            IEnumerable<string> y = new[] {"eins", "drei", "zwei"}.ToImmutableList().AsValueObject();
            x.Equals(y).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_IsTrueForSameElements()
        {
            IEnumerable<string> x = new[] {"eins", "zwei", null, "drei"}.ToImmutableList().AsValueObject();
            IEnumerable<string> y = new[] {"eins", "zwei", null, "drei"}.ToImmutableList().AsValueObject();
            x.GetHashCode().Should().Be(y.GetHashCode());
        }

        [Fact]
        public void GetHashCode_IsFalseForDifferentElements()
        {
            IEnumerable<string> x = new[] {"eins", "zwei", "drei"}.ToImmutableList().AsValueObject();
            IEnumerable<string> y = new[] {"eins", "zwei", "vier"}.ToImmutableList().AsValueObject();
            x.GetHashCode().Should().NotBe(y.GetHashCode());
        }
    }
}
