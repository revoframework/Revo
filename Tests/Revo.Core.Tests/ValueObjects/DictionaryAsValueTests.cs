using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Revo.Core.ValueObjects;
using Xunit;

namespace Revo.Core.Tests.ValueObjects
{
    public class DictionaryAsValueTests
    {
        [Fact]
        public void Equals_IsTrueForSameElements()
        {
            IReadOnlyDictionary<string, int> x = new Dictionary<string, int> { { "one", 1 }, { "two", 2 } }.ToImmutableDictionary().AsValueObject();
            IReadOnlyDictionary<string, int> y = new Dictionary<string, int> { { "one", 1 }, { "two", 2 } }.ToImmutableDictionary().AsValueObject();
            x.Equals(y).Should().BeTrue();
        }

        [Theory]
        [InlineData(new string[] { }, new int[] { })]
        [InlineData(new[]{ "one" }, new[]{ 1 })]
        [InlineData(new[]{ "one", "two", "three" }, new[]{ 1, 2, 3 })]
        [InlineData(new[]{ "one", "two" }, new[]{ 1, 3 })]
        public void Equals_IsFalseForDifferentElements(string[] keys, int[] values)
        {
            IReadOnlyDictionary<string, int> x = new Dictionary<string, int> { { "one", 1 }, { "two", 2 } }.ToImmutableDictionary().AsValueObject();
            IReadOnlyDictionary<string, int> y =
                new Dictionary<string, int>(keys.Zip(values, (k, v) => new KeyValuePair<string, int>(k, v)))
                    .ToImmutableDictionary()
                    .AsValueObject();

            x.Equals(y).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_IsTrueForSameElements()
        {
            IReadOnlyDictionary<string, int> x = new Dictionary<string, int> { { "one", 1 }, { "two", 2 } }.ToImmutableDictionary().AsValueObject();
            IReadOnlyDictionary<string, int> y = new Dictionary<string, int> { { "one", 1 }, { "two", 2 } }.ToImmutableDictionary().AsValueObject();
            x.GetHashCode().Should().Be(y.GetHashCode());
        }

        [Theory]
        [InlineData(new string[] { }, new int[] { })]
        [InlineData(new[] { "one" }, new[] { 1 })]
        [InlineData(new[] { "one", "two", "three" }, new[] { 1, 2, 3 })]
        [InlineData(new[] { "one", "two" }, new[] { 1, 3 })]
        public void GetHashCode_IsFalseForDifferentElements(string[] keys, int[] values)
        {
            IReadOnlyDictionary<string, int> x = new Dictionary<string, int> { { "one", 1 }, { "two", 2 } }.ToImmutableDictionary().AsValueObject();
            IReadOnlyDictionary<string, int> y =
                new Dictionary<string, int>(keys.Zip(values, (k, v) => new KeyValuePair<string, int>(k, v)))
                    .ToImmutableDictionary()
                    .AsValueObject();

            x.GetHashCode().Should().NotBe(y.GetHashCode());
        }
    }
}
