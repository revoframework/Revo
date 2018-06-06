using System.Collections.Generic;
using System.Collections.Immutable;
using FluentAssertions;
using Revo.Core.ValueObjects;
using Xunit;

namespace Revo.Core.Tests.ValueObjects
{
    public class ValueObjectTests
    {
        [Fact]
        public void Equals_TrueForTwoSame()
        {
            var x = new TestValue("Foo", 42);
            var y = new TestValue("Foo", 42);

            x.Equals(y).Should().BeTrue();
            x.Equals((object)y).Should().BeTrue();
        }

        [Theory]
        [InlineData("Foo", 42, "Bar", 42)]
        [InlineData("Bar", 41, "Bar", 42)]
        public void Equals_FalseForDifferent(string xFoo, int xBar, string yFoo, int yBar)
        {
            var x = new TestValue(xFoo, xBar);
            var y = new TestValue(yFoo, yBar);

            x.Equals(y).Should().BeFalse();
            x.Equals((object)y).Should().BeFalse();
        }
        
        [Fact]
        public void Equals_FalseForNull()
        {
            var x = new TestValue("Foo", 42);

            x.Equals((TestValue)null).Should().BeFalse();
            x.Equals((object)null).Should().BeFalse();
        }

        [Fact]
        public void Equals_FalseForDifferentTypes()
        {
            var x = new TestValue("Foo", 42);
            var y = new TestValue2("Foo", 42);

            x.Equals(y).Should().BeFalse();
        }

        [Fact]
        public void Equals_HandlesPropertyNulls()
        {
            var x = new TestValue(null, 42);
            var y = new TestValue("Foo", 42);
            var z = new TestValue(null, 42);

            x.Equals(y).Should().BeFalse();
            x.Equals((object)y).Should().BeFalse();

            x.Equals(z).Should().BeTrue();
            x.Equals((object)z).Should().BeTrue();
        }

        [Fact]
        public void Equals_SameWithRepeatedCallsAndCachesValueComponents()
        {
            var x = new TestValue("Foo", 42);
            var y = new TestValue("Foo", 42);

            x.Equals(y).Should().BeTrue();
            x.Equals((object)y).Should().BeTrue();
            x.Equals(y).Should().BeTrue();
            x.Equals((object)y).Should().BeTrue();

            x.GetValueComponentsCount.Should().Be(1);
        }

        [Fact]
        public void Equals_DoesNotCompareOtherProperties()
        {
            var x = new TestValue3("Foo", 42);
            var y = new TestValue3("Foo", 43);

            x.Equals(y).Should().BeTrue();
            x.Equals((object)y).Should().BeTrue();
        }

        [Theory]
        [InlineData(true, "one", "two")]
        [InlineData(false, "one", "three")]
        public void Equals_WorksWithCustomTypes(bool shouldMatch, params string[] yElements)
        {
            var x = new TestValue4(new[] {"one", "two"}.ToImmutableList());
            var y = new TestValue4(yElements.ToImmutableList());

            x.Equals(y).Should().Be(shouldMatch);
            x.Equals((object)y).Should().Be(shouldMatch);
        }

        [Fact]
        public void GetHashCode_EqualForTwoSame()
        {
            var x = new TestValue("Foo", 42);
            var y = new TestValue("Foo", 42);

            x.GetHashCode().Should().Be(y.GetHashCode());
        }

        [Theory]
        [InlineData("Foo", 42, "Bar", 42)]
        [InlineData("Bar", 41, "Bar", 42)]
        public void GetHashCode_NotEqualForDifferentComponents(string xFoo, int xBar, string yFoo, int yBar)
        {
            var x = new TestValue(xFoo, xBar);
            var y = new TestValue(yFoo, yBar);

            x.GetHashCode().Should().NotBe(y.GetHashCode());
        }

        [Fact]
        public void GetHashCode_HandlesPropertyNulls()
        {
            var x = new TestValue(null, 42);
            var y = new TestValue("Foo", 42);

            x.GetHashCode().Should().NotBe(y.GetHashCode());
        }

        [Fact]
        public void GetHashCode_SameWithRepeatedCallsAndCachesValueComponents()
        {
            var x = new TestValue("Foo", 42);

            int res1 = x.GetHashCode();
            int res2 = x.GetHashCode();

            res1.Should().Be(res2);
            x.GetValueComponentsCount.Should().Be(1);
        }

        [Fact]
        public void GetHashCode_DoesNotCompareOtherProperties()
        {
            var x = new TestValue3("Foo", 42);
            var y = new TestValue3("Foo", 43);

            x.GetHashCode().Should().Be(y.GetHashCode());
        }

        [Theory]
        [InlineData(true, "one", "two")]
        [InlineData(false, "one", "three")]
        public void GetHashCode_WorksWithCustomTypes(bool shouldMatch, params string[] yElements)
        {
            var x = new TestValue4(new[] { "one", "two" }.ToImmutableList());
            var y = new TestValue4(yElements.ToImmutableList());

            if (shouldMatch)
            {
                x.GetHashCode().Should().Be(y.GetHashCode());
            }
            else
            {
                x.GetHashCode().Should().NotBe(y.GetHashCode());
            }
        }

        [Theory]
        [InlineData("hello", 1, "TestValue2 { Foo = hello, Bar = 1 }")]
        [InlineData(null, 1, "TestValue2 { Foo = null, Bar = 1 }")]
        public void ToString_FormatsComponents(string foo, int bar, string toString)
        {
            var x = new TestValue2(foo, bar);
            x.ToString().Should().Be(toString);
        }

        public class TestValue : ValueObject<TestValue>
        {
            public TestValue(string foo, int bar)
            {
                Foo = foo;
                Bar = bar;
            }

            public string Foo { get; }
            public int Bar { get; }

            public int GetValueComponentsCount { get; private set; }

            protected override IEnumerable<(string Name, object Value)> GetValueComponents()
            {
                GetValueComponentsCount++;
                yield return (nameof(Foo), Foo);
                yield return (nameof(Bar), Bar);
            }
        }

        public class TestValue2 : ValueObject<TestValue2>
        {
            public TestValue2(string foo, int bar)
            {
                Foo = foo;
                Bar = bar;
            }

            public string Foo { get; }
            public int Bar { get; }

            protected override IEnumerable<(string Name, object Value)> GetValueComponents()
            {
                yield return (nameof(Foo), Foo);
                yield return (nameof(Bar), Bar);
            }
        }

        public class TestValue3 : ValueObject<TestValue3>
        {
            public TestValue3(string foo, int bar)
            {
                Foo = foo;
                Bar = bar;
            }

            public string Foo { get; }
            public int Bar { get; }

            protected override IEnumerable<(string Name, object Value)> GetValueComponents()
            {
                yield return (nameof(Foo), Foo);
            }
        }

        public class TestValue4 : ValueObject<TestValue4>
        {
            public TestValue4(ImmutableList<string> foo)
            {
                Foo = foo;
            }

            public ImmutableList<string> Foo { get; }

            protected override IEnumerable<(string Name, object Value)> GetValueComponents()
            {
                yield return (nameof(Foo), Foo.AsValueObject());
            }
        }
    }
}
