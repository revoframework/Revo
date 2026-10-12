using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Services;
using Revo.Core.Commands;
using NSubstitute;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class LocalCommandBusTests
    {
        private readonly LocalCommandBus sut;
        private readonly IVariantServiceResolver serviceResolver;
        private readonly ICommandBusPipeline commandBusPipeline;

        public LocalCommandBusTests()
        {
            serviceResolver = Substitute.For<IVariantServiceResolver>();
            commandBusPipeline = Substitute.For<ICommandBusPipeline>();
            sut = new LocalCommandBus(serviceResolver, commandBusPipeline);
        }

        [Fact]
        public async Task SendAsync_SelectsCorrectHandler()
        {
            var commandHandler = Substitute.For<ICommandHandler<TestCommand>>();
            serviceResolver.GetService(typeof(ICommandHandler<TestCommand>)).Returns(commandHandler);

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
            serviceResolver.GetService(typeof(ICommandHandler<TestQuery, int>)).Returns(commandHandler);

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
            serviceResolver.GetService(typeof(ICommandHandler<TestQuery, int>)).Returns(commandHandler);

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
            serviceResolver.GetService(typeof(ICommandHandler<TestCommand>)).Returns(commandHandler);

            var command = new TestCommand();
            var cancellationToken = new CancellationToken();
            var executionOptions = new CommandExecutionOptions();

            commandBusPipeline
                .ProcessAsync(command, Arg.Any<CommandBusMiddlewareDelegate>(),
                    sut, executionOptions, cancellationToken)
                .Returns(ci =>
                {
                    serviceResolver.DidNotReceive().GetService(typeof(ICommandHandler<TestCommand>));
                    var result = ci.ArgAt<CommandBusMiddlewareDelegate>(1)(command, cancellationToken);
                    serviceResolver.Received(1).GetService(typeof(ICommandHandler<TestCommand>));
                    return result;
                });

            await sut.SendAsync(command, executionOptions, cancellationToken);

            commandHandler.Received(1).HandleAsync(command, cancellationToken);
        }

        [Fact]
        public async Task SendAsync_UsesHandlerOfBaseCommandType()
        {
            var commandHandler = new BaseTestCommandHandler();
            var services = new ServiceCollection();
            services.AddSingleton<ICommandHandler<BaseTestCommand>>(commandHandler);

            var bus = new LocalCommandBus(
                new VariantServiceResolver(services.BuildServiceProvider(), new VariantServiceTypeIndex(services)),
                commandBusPipeline);

            var command = new DerivedTestCommand();
            var cancellationToken = new CancellationToken();
            var executionOptions = new CommandExecutionOptions();

            commandBusPipeline
                .ProcessAsync(command, Arg.Any<CommandBusMiddlewareDelegate>(),
                    bus, executionOptions, cancellationToken)
                .Returns(ci => ci.ArgAt<CommandBusMiddlewareDelegate>(1)(command, cancellationToken));

            await bus.SendAsync(command, executionOptions, cancellationToken);

            commandHandler.HandledCommands.Should().Equal(command);
        }

        public class TestCommand : ICommand
        {
        }

        public class BaseTestCommand : ICommand
        {
        }

        public class DerivedTestCommand : BaseTestCommand
        {
        }

        public class BaseTestCommandHandler : ICommandHandler<BaseTestCommand>
        {
            public List<BaseTestCommand> HandledCommands { get; } = new();

            public Task HandleAsync(BaseTestCommand command, CancellationToken cancellationToken)
            {
                HandledCommands.Add(command);
                return Task.CompletedTask;
            }
        }

        public class TestQuery : IQuery<int>
        {
        }
    }
}