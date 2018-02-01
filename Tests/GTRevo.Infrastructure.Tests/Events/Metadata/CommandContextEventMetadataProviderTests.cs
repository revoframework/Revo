using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GTRevo.Core.Commands;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Events.Metadata;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Events.Metadata
{
    public class CommandContextEventMetadataProviderTests
    {
        private readonly ICommandContext commandContext;
        private readonly CommandContextEventMetadataProvider sut;

        public CommandContextEventMetadataProviderTests()
        {
            commandContext = Substitute.For<ICommandContext>();
            sut = new CommandContextEventMetadataProvider(commandContext);
        }

        [Fact]
        public async Task GetMetadataAsync_ReturnsCommandData()
        {
            commandContext.CurrentCommand.Returns(new TestCommand());

            var result = await sut.GetMetadataAsync(Substitute.For<IEventMessage>());

            result.Should().HaveCount(1);
            //result.Should().Contain((BasicEventMetadataNames.CommandId, typeof(TestCommand).FullName)); // TODO
            result.Should().Contain((BasicEventMetadataNames.CommandTypeId, typeof(TestCommand).FullName));
        }

        [Fact]
        public async Task GetMetadataAsync_EmptyWhenNoCommand()
        {
            commandContext.CurrentCommand.Returns((ICommandBase)null);
            var result = await sut.GetMetadataAsync(Substitute.For<IEventMessage>());
            result.Should().BeEmpty();
        }

        public class TestCommand : ICommand
        {
        }
    }
}
