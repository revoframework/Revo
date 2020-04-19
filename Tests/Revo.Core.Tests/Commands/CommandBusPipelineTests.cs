using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Commands;
using NSubstitute;
using Revo.Core.Core;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class CommandBusPipelineTests
    {
        private readonly CommandBusPipeline sut;
        private readonly ICommandBusMiddlewareFactory middlewareFactory;
        private readonly ICommandBus commandBus;

        public CommandBusPipelineTests()
        {
            middlewareFactory = Substitute.For<ICommandBusMiddlewareFactory>();
            commandBus = Substitute.For<ICommandBus>();
            sut = new CommandBusPipeline(middlewareFactory);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ProcessAsync_MiddlewaresInvokedInOrder(bool reverseMiddlewares)
        {
            var middlewares = new[]
            {
                Substitute.For<ICommandBusMiddleware<MyCommand>>(),
                Substitute.For<ICommandBusMiddleware<MyCommand>>()
            };

            var command = new MyCommand();
            var command2 = new MyCommand();
            var command3 = new MyCommand();
            var cancellationToken = new CancellationToken();
            var executionOptions = new CommandExecutionOptions();
            var executionHandler = Substitute.For<CommandBusMiddlewareDelegate>();

            middlewares[0].Order = 1;
            middlewares[0]
                .When(x => x.HandleAsync(command, executionOptions,
                    Arg.Any<CommandBusMiddlewareDelegate>(), cancellationToken))
                .Do(ci =>
                {
                    middlewares[1].DidNotReceiveWithAnyArgs().HandleAsync(null, null, null, CancellationToken.None);
                    executionHandler.DidNotReceiveWithAnyArgs().Invoke(null);

                    var next = ci.ArgAt<CommandBusMiddlewareDelegate>(2);
                    next(command2);
                });
            
            middlewares[1].Order = 2;
            middlewares[1]
                .When(x => x.HandleAsync(command2, executionOptions,
                    Arg.Any<CommandBusMiddlewareDelegate>(), cancellationToken))
                .Do(ci =>
                {
                    middlewares[0].Received(1).HandleAsync(command, executionOptions,
                        Arg.Any<CommandBusMiddlewareDelegate>(), cancellationToken);
                    executionHandler.DidNotReceiveWithAnyArgs().Invoke(null);

                    var next = ci.ArgAt<CommandBusMiddlewareDelegate>(2);
                    next(command3);
                });

            middlewareFactory.CreateMiddlewares<MyCommand>(commandBus)
                .Returns(reverseMiddlewares ? middlewares.Reverse().ToArray() : middlewares);

            await sut.ProcessAsync(command, executionHandler, commandBus, executionOptions,
                cancellationToken);

            middlewares[0].Received(1).HandleAsync(command, executionOptions,
                Arg.Any<CommandBusMiddlewareDelegate>(), cancellationToken);
            middlewares[1].Received(1).HandleAsync(command2, executionOptions,
                Arg.Any<CommandBusMiddlewareDelegate>(), cancellationToken);
            executionHandler.Received(1).Invoke(command3);
        }
        
        [Fact]
        public async Task ProcessAsync_WhenNoMiddleware()
        {
            var middlewares = new ICommandBusMiddleware<MyCommand>[0];

            middlewareFactory.CreateMiddlewares<MyCommand>(commandBus)
                .Returns(middlewares);

            var command = new MyCommand();
            var cancellationToken = new CancellationToken();
            var executionOptions = new CommandExecutionOptions();
            var executionHandler = Substitute.For<CommandBusMiddlewareDelegate>();
            
            await sut.ProcessAsync(command, executionHandler, commandBus, executionOptions,
                cancellationToken);

            executionHandler.Received(1).Invoke(command);
        }
        
        [Fact]
        public async Task Handle_RunsInNewTaskScope()
        {
            TaskContext middlewareContext = null;

            var middlewares = new[]
            {
                Substitute.For<ICommandBusMiddleware<MyCommand>>()
            };

            var command = new MyCommand();
            var cancellationToken = new CancellationToken();
            var executionOptions = new CommandExecutionOptions();
            var executionHandler = Substitute.For<CommandBusMiddlewareDelegate>();

            middlewareFactory.CreateMiddlewares<MyCommand>(commandBus)
                .Returns(middlewares);

            middlewares[0]
                .When(x => x.HandleAsync(command, executionOptions,
                    Arg.Any<CommandBusMiddlewareDelegate>(), cancellationToken))
                .Do(ci => middlewareContext = TaskContext.Current);

            var beforeContext = TaskContext.Current;
            await sut.ProcessAsync(command, executionHandler,
                commandBus, executionOptions, cancellationToken);
            var afterContext = TaskContext.Current;

            beforeContext.Should().BeSameAs(afterContext);
            beforeContext.Should().NotBeSameAs(middlewareContext);
            middlewareContext.Should().NotBeNull();
        }

        public class MyCommand : ICommand
        {
        }
    }
}
