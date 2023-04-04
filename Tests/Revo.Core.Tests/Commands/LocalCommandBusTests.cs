using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Commands;
using NSubstitute;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class LocalCommandBusTests
    {
        private readonly LocalCommandBus sut;
        private readonly IServiceProvider serviceProvider;
        private readonly ICommandBusPipeline commandBusPipeline;

        public LocalCommandBusTests()
        {
            serviceProvider = Substitute.For<IServiceProvider>();
            commandBusPipeline = Substitute.For<ICommandBusPipeline>();
            sut = new LocalCommandBus(serviceProvider, commandBusPipeline);
        }

        [Fact]
        public async Task SendAsync_SelectsCorrectHandler()
        {
            var commandHandler = Substitute.For<ICommandHandler<TestCommand>>();
            serviceProvider.GetService(typeof(ICommandHandler<TestCommand>)).Returns(commandHandler);

            var command = new TestCommand();
            var cancellationToken = new CancellationToken();
            var executionOptions = new CommandExecutionOptions();

            commandBusPipeline
                .ProcessAsync(command, Arg.Any<CommandBusMiddlewareDelegate>(),
                    sut, executionOptions, cancellationToken)
                .Returns(ci => ci.ArgAt<CommandBusMiddlewareDelegate>(1)(command, cancellationToken));

            await sut.SendAsync(command, executionOptions, cancellationToken);

            commandHandler.Received(1).HandleAsync(command, cancellationToken);
        }

        [Fact]
        public async Task SendAsync_ReturnsFromQuery()
        {
            var commandHandler = Substitute.For<ICommandHandler<TestQuery, int>>();
            serviceProvider.GetService(typeof(ICommandHandler<TestQuery, int>)).Returns(commandHandler);

            var command = new TestQuery();
            var cancellationToken = new CancellationToken();
            var executionOptions = new CommandExecutionOptions();
            commandHandler.HandleAsync(command, cancellationToken).Returns(42);

            commandBusPipeline
                .ProcessAsync(command, Arg.Any<CommandBusMiddlewareDelegate>(),
                    sut, executionOptions, cancellationToken)
                .Returns(ci => ci.ArgAt<CommandBusMiddlewareDelegate>(1)(command, cancellationToken));

            int result = await sut.SendAsync(command, executionOptions, cancellationToken);

            result.Should().Be(42);
        }

        [Fact]
        public async Task SendAsync_QueryAsICommandBase()
        {
            var commandHandler = Substitute.For<ICommandHandler<TestQuery, int>>();
            serviceProvider.GetService(typeof(ICommandHandler<TestQuery, int>)).Returns(commandHandler);

            var command = new TestQuery();
            var cancellationToken = new CancellationToken();
            var executionOptions = new CommandExecutionOptions();
            commandHandler.HandleAsync(command, cancellationToken).Returns(42);

            commandBusPipeline
                .ProcessAsync(command, Arg.Any<CommandBusMiddlewareDelegate>(),
                    sut, executionOptions, cancellationToken)
                .Returns(ci => ci.ArgAt<CommandBusMiddlewareDelegate>(1)(command, cancellationToken));

            await sut.SendAsync((ICommandBase) command, executionOptions, cancellationToken);

            commandHandler.Received(1).HandleAsync(command, cancellationToken);
        }

        [Fact]
        public async Task CommandHandlerIsResolvedOnlyInsideExecutionHandler()
        {
            var commandHandler = Substitute.For<ICommandHandler<TestCommand>>();
            serviceProvider.GetService(typeof(ICommandHandler<TestCommand>)).Returns(commandHandler);

            var command = new TestCommand();
            var cancellationToken = new CancellationToken();
            var executionOptions = new CommandExecutionOptions();

            commandBusPipeline
                .ProcessAsync(command, Arg.Any<CommandBusMiddlewareDelegate>(),
                    sut, executionOptions, cancellationToken)
                .Returns(ci =>
                {
                    serviceProvider.DidNotReceive().GetService(typeof(ICommandHandler<TestCommand>));
                    var result = ci.ArgAt<CommandBusMiddlewareDelegate>(1)(command, cancellationToken);
                    serviceProvider.Received(1).GetService(typeof(ICommandHandler<TestCommand>));
                    return result;
                });

            await sut.SendAsync(command, executionOptions, cancellationToken);

            commandHandler.Received(1).HandleAsync(command, cancellationToken);
        }

        public class TestCommand : ICommand
        {
        }

        public class TestQuery : IQuery<int>
        {
        }
    }
}