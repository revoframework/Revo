using FluentAssertions;
using NSubstitute;
using Revo.Core.Commands;
using Revo.Core.Types;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class CommandTypeDiscoveryTests
    {
        private CommandTypeDiscovery sut;
        private ITypeExplorer typeExplorer;

        public CommandTypeDiscoveryTests()
        {
            typeExplorer = Substitute.For<ITypeExplorer>();
            sut = new CommandTypeDiscovery(typeExplorer);
        }

        [Fact]
        public void Discover_Empty()
        {
            typeExplorer.GetAllTypes().Returns(new[] {typeof(UnrelevantClass)});
            sut.DiscoverCommandTypes().Should().BeEmpty();
        }

        [Fact]
        public void Discover_FindsCommandType()
        {
            typeExplorer.GetAllTypes().Returns(new[] { typeof(TestCommand) });
            sut.DiscoverCommandTypes().Should().BeEquivalentTo(new[] { typeof(TestCommand) });
        }

        [Fact]
        public void Discover_FindsGenericCommandType()
        {
            typeExplorer.GetAllTypes().Returns(new[] { typeof(TestGenericCommand<>) });
            sut.DiscoverCommandTypes().Should().BeEquivalentTo(new[] { typeof(TestGenericCommand<>) });
        }

        private class UnrelevantClass
        {
        }

        private class TestCommand : ICommand
        {
        }

        private class TestGenericCommand<T> : ICommand
        {
            public T Param { get; init; }
        }
    }
}