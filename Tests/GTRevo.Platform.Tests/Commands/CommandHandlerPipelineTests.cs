using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Core.Commands;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace GTRevo.Platform.Tests.Commands
{
    public class CommandHandlerPipelineTests
    {
        private readonly IAsyncCommandHandler<MyCommand> sut;
        private readonly List<ICommandFilter<MyCommand>> filters = new List<ICommandFilter<MyCommand>>();
        private readonly IAsyncCommandHandler<MyCommand> decorated;

        public CommandHandlerPipelineTests()
        {
            var filter = Substitute.For<ICommandFilter<MyCommand>>();
            filters.Add(filter);

            decorated = Substitute.For<IAsyncCommandHandler<MyCommand>>();

            sut = new CommandHandlerPipeline<MyCommand>(filters.ToArray(), decorated);
        }

        [Fact]
        public async Task Handle_CallsFilters()
        {
            var command = new MyCommand();

            await sut.Handle(command);

            Received.InOrder(()
                =>
            {
                filters[0].Handle(command);
                decorated.Handle(command);
            });
        }

        [Fact]
        public async Task Handle_DoesntCallDecoratedWhenFilterThrows()
        {
            var command = new MyCommand();

            filters[0].Handle(command).Throws(new Exception());

            await Assert.ThrowsAsync<Exception>(() =>
                sut.Handle(command));

            decorated.DidNotReceive().Handle(command);
        }

        public class MyCommand : ICommand
        {
        }
    }
}
