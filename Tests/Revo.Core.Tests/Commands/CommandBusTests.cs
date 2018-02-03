using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Commands;
using Revo.Core.Core;
using NSubstitute;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class CommandBusTests
    {
        private readonly ICommandBus sut;
        private readonly IServiceLocator serviceLocator;

        public CommandBusTests()
        {
            serviceLocator = Substitute.For<IServiceLocator>();
            sut = new CommandBus(serviceLocator);
        }

        [Fact]
        public void SendAsync_SelectsCorrectHandler()
        {
            var commandHandler = Substitute.For<ICommandHandler<TestCommand>>();
            serviceLocator.Get(typeof(ICommandHandler<TestCommand>)).Returns(commandHandler);

            var command = new TestCommand();
            var cancellationToken = new CancellationToken();
            Task returnTask = Task.FromResult(0);
            commandHandler.HandleAsync(command, cancellationToken).Returns(returnTask);

            Task result = sut.SendAsync(command, cancellationToken);

            result.Should().Be(returnTask); // TODO test better that has been awaited?
        }

        [Fact]
        public async Task SendAsync_ReturnsFromQuery()
        {
            var commandHandler = Substitute.For<ICommandHandler<TestQuery, int>>();
            serviceLocator.Get(typeof(ICommandHandler<TestQuery, int>)).Returns(commandHandler);

            var command = new TestQuery();
            var cancellationToken = new CancellationToken();
            commandHandler.HandleAsync(command, cancellationToken).Returns(42);

            int result = await sut.SendAsync(command, cancellationToken);

            result.Should().Be(42);
        }

        [Fact]
        public void SendAsync_QueryAsICommandBase()
        {
            var commandHandler = Substitute.For<ICommandHandler<TestQuery, int>>();
            serviceLocator.Get(typeof(ICommandHandler<TestQuery, int>)).Returns(commandHandler);

            var command = new TestQuery();
            var cancellationToken = new CancellationToken();
            Task returnTask = Task.FromResult(42);
            commandHandler.HandleAsync(command, cancellationToken).Returns(returnTask);

            Task result = sut.SendAsync((ICommandBase) command, cancellationToken);

            result.Should().Be(returnTask); // TODO test better that has been awaited?
        }

        public class TestCommand : ICommand
        {
        }

        public class TestQuery : IQuery<int>
        {
        }
    }
}
