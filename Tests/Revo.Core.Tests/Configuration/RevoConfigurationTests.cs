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
        public void GetSection_CreatesNewInstance()
        {
            var section = sut.GetSection<TestSection>();
            section.Should().NotBeNull();
            section.Should().BeOfType<TestSection>();
            section.Foo.Should().Be(5);
        }

        [Fact]
        public void GetSection_TwiceReturnsSame()
        {
            var a = sut.GetSection<TestSection>();
            var b = sut.GetSection<TestSection>();
            a.Should().Be(b);
        }

        public class TestSection : IRevoConfigurationSection
        {
            public int Foo { get; set; } = 5;
        }
    }
}
