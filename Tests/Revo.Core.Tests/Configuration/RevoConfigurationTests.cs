using System;
using FluentAssertions;
using Revo.Core.Configuration;
using Xunit;

namespace Revo.Core.Tests.Configuration
{
    public class RevoConfigurationTests
    {
        private RevoConfiguration sut;

        public RevoConfigurationTests()
        {
            sut = new RevoConfiguration();
        }

        [Fact]
        public void GetOrAddSection_CreatesNewInstance()
        {
            var section = sut.GetOrAddSection(_ => new TestSection());
            section.Should().NotBeNull();
            section.Should().BeOfType<TestSection>();
            section.Foo.Should().Be(5);
        }

        [Fact]
        public void GetOrAddSection_TwiceReturnsSame()
        {
            var a = sut.GetOrAddSection(_ => new TestSection());
            var b = sut.GetOrAddSection(_ => new TestSection());
            a.Should().Be(b);
        }

        [Fact]
        public void GetSection_ThrowsWhenNotAdded()
        {
            var act = () => sut.GetSection<TestSection>();
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*not added yet*");
        }

        public class TestSection : IRevoConfigurationSection
        {
            public int Foo { get; set; } = 5;
        }
    }
}
