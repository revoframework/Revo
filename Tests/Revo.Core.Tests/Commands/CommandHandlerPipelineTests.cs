using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class CommandHandlerPipelineTests
    {
        private readonly ICommandHandler<MyCommand> sut;
        private readonly List<IPreCommandFilter<MyCommand>> preFilters = new List<IPreCommandFilter<MyCommand>>();
        private readonly List<IPostCommandFilter<MyCommand>> postFilters = new List<IPostCommandFilter<MyCommand>>();
        private readonly List<IExceptionCommandFilter<MyCommand>> exceptionFilters = new List<IExceptionCommandFilter<MyCommand>>();
        private readonly ICommandHandler<MyCommand> decorated;
        private readonly Func<ICommandHandler<MyCommand>> decoratedFunc;

        public CommandHandlerPipelineTests()
        {
            var preFilter = Substitute.For<IPreCommandFilter<MyCommand>>();
            preFilters.Add(preFilter);

            var postFilter = Substitute.For<IPostCommandFilter<MyCommand>>();
            postFilters.Add(postFilter);

            var exceptionFilter = Substitute.For<IExceptionCommandFilter<MyCommand>>();
            exceptionFilters.Add(exceptionFilter);

            decorated = Substitute.For<ICommandHandler<MyCommand>>();
            decoratedFunc = Substitute.For<Func<ICommandHandler<MyCommand>>>();
            decoratedFunc().Returns(decorated);

            sut = new CommandHandlerPipeline<MyCommand>(preFilters.ToArray(),
                postFilters.ToArray(), exceptionFilters.ToArray(), decoratedFunc);
        }

        [Fact]
        public async Task Handle_CallsPreFilters()
        {
            var command = new MyCommand();
            var cancellationToken = new CancellationToken();

            await sut.HandleAsync(command, cancellationToken);

            Received.InOrder(()
                =>
            {
                preFilters[0].PreFilterAsync(command);
                decorated.HandleAsync(command, cancellationToken);
            });
        }

        [Fact]
        public async Task Handle_DoesntCallDecoratedWhenPreFilterThrows()
        {
            var command = new MyCommand();

            preFilters[0].PreFilterAsync(command).Throws(new Exception());
            
            await Assert.ThrowsAsync<Exception>(() =>
                sut.HandleAsync(command, new CancellationToken()));

            decoratedFunc.DidNotReceive().Invoke();
            decorated.DidNotReceiveWithAnyArgs().HandleAsync(command, new CancellationToken()); // probably redundant with previous line
        }

        [Fact]
        public async Task Handle_CallsPostFilters()
        {
            var command = new MyCommand();
            var cancellationToken = new CancellationToken();

            await sut.HandleAsync(command, cancellationToken);

            Received.InOrder(()
                =>
            {
                decorated.HandleAsync(command, cancellationToken);
                postFilters[0].PostFilterAsync(command, null);
            });
        }
        
        [Fact]
        public async Task Handle_CallsExceptionFilters()
        {
            var command = new MyCommand();
            var cancellationToken = new CancellationToken();
            Exception e = new Exception();
            decorated.HandleAsync(command, cancellationToken).Throws(e);

            await Assert.ThrowsAsync<Exception>(() =>
                sut.HandleAsync(command, cancellationToken));

            decorated.Received(1).HandleAsync(command, cancellationToken);
            exceptionFilters[0].Received(1).FilterExceptionAsync(command, e);
            postFilters[0].DidNotReceiveWithAnyArgs().PostFilterAsync(null, null);
        }

        public class MyCommand : ICommand
        {
        }
    }
}
