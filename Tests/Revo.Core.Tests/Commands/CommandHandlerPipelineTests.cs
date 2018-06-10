using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Commands;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Revo.Core.Core;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class CommandHandlerPipelineTests
    {
        private readonly ICommandHandler<MyCommand> sut;
        private readonly List<IPreCommandFilter<MyCommand>> preFilters = new List<IPreCommandFilter<MyCommand>>();
        private readonly Func<IPreCommandFilter<MyCommand>[]> preFiltersFunc;
        private readonly List<IPostCommandFilter<MyCommand>> postFilters = new List<IPostCommandFilter<MyCommand>>();
        private readonly Func<IPostCommandFilter<MyCommand>[]> postFiltersFunc;
        private readonly List<IExceptionCommandFilter<MyCommand>> exceptionFilters = new List<IExceptionCommandFilter<MyCommand>>();
        private readonly Func<IExceptionCommandFilter<MyCommand>[]> exceptionFiltersFunc;
        private readonly ICommandHandler<MyCommand> decorated;
        private readonly Func<ICommandHandler<MyCommand>> decoratedFunc;

        public CommandHandlerPipelineTests()
        {
            var preFilter = Substitute.For<IPreCommandFilter<MyCommand>>();
            preFilters.Add(preFilter);
            preFiltersFunc = Substitute.For<Func<IPreCommandFilter<MyCommand>[]>>();
            preFiltersFunc.Invoke().Returns(preFilters.ToArray());

            var postFilter = Substitute.For<IPostCommandFilter<MyCommand>>();
            postFilters.Add(postFilter);
            postFiltersFunc = Substitute.For<Func<IPostCommandFilter<MyCommand>[]>>();
            postFiltersFunc.Invoke().Returns(postFilters.ToArray());

            var exceptionFilter = Substitute.For<IExceptionCommandFilter<MyCommand>>();
            exceptionFilters.Add(exceptionFilter);
            exceptionFiltersFunc = Substitute.For<Func<IExceptionCommandFilter<MyCommand>[]>>();
            exceptionFiltersFunc.Invoke().Returns(exceptionFilters.ToArray());

            decorated = Substitute.For<ICommandHandler<MyCommand>>();
            decoratedFunc = Substitute.For<Func<ICommandHandler<MyCommand>>>();
            decoratedFunc().Returns(decorated);

            sut = new CommandHandlerPipeline<MyCommand>(preFiltersFunc, postFiltersFunc,
                exceptionFiltersFunc, decoratedFunc);
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
            InvalidOperationException e = new InvalidOperationException();
            decorated.HandleAsync(command, cancellationToken).Throws(e);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.HandleAsync(command, cancellationToken));

            decorated.Received(1).HandleAsync(command, cancellationToken);
            exceptionFilters[0].Received(1).FilterExceptionAsync(command, e);
            postFilters[0].DidNotReceiveWithAnyArgs().PostFilterAsync(null, null);
        }

        [Fact]
        public async Task Handle_RunsInNewTaskScope()
        {
            var command = new MyCommand();
            
            TaskContext preFilterConstructContext = null;
            TaskContext preFilterContext = null;
            TaskContext handlerConstructContext = null;
            TaskContext handlerContext = null;
            TaskContext postFilterContext = null;
            TaskContext postFilterConstructContext = null;
            preFiltersFunc.WhenForAnyArgs(x => x.Invoke()).Do(ci => preFilterConstructContext = TaskContext.Current);
            preFilters[0].WhenForAnyArgs(x => x.PreFilterAsync(command)).Do(ci => preFilterContext = TaskContext.Current);
            decoratedFunc.WhenForAnyArgs(x => x.Invoke()).Do(ci => handlerConstructContext = TaskContext.Current);
            decorated.WhenForAnyArgs(x => x.HandleAsync(command, CancellationToken.None)).Do(ci => handlerContext = TaskContext.Current);
            postFiltersFunc.WhenForAnyArgs(x => x.Invoke()).Do(ci => postFilterConstructContext = TaskContext.Current);
            postFilters[0].WhenForAnyArgs(x => x.PostFilterAsync(command, null)).Do(ci => postFilterContext = TaskContext.Current);

            var beforeContext = TaskContext.Current;
            await sut.HandleAsync(command, new CancellationToken());
            var afterContext = TaskContext.Current;

            beforeContext.Should().BeSameAs(afterContext);
            beforeContext.Should().NotBeSameAs(preFilterConstructContext);
            preFilterConstructContext.Should().NotBeNull();

            new[] { preFilterContext, handlerConstructContext, handlerContext, postFilterContext, postFilterConstructContext }
                .Should().AllBeEquivalentTo(preFilterConstructContext);
        }

        [Fact]
        public async Task Handle_ExceptionFilterCreatedInNewTaskScope()
        {
            var command = new MyCommand();

            var e = new InvalidOperationException();
            decorated.HandleAsync(command, CancellationToken.None).Throws(e);

            TaskContext handlerContext = null;
            TaskContext exceptionFilterConstructContext = null;
            TaskContext exceptionFilterContext = null;
            decorated.WhenForAnyArgs(x => x.HandleAsync(null, CancellationToken.None)).Do(ci => handlerContext = TaskContext.Current);
            exceptionFiltersFunc.WhenForAnyArgs(x => x.Invoke()).Do(ci => exceptionFilterConstructContext = TaskContext.Current);
            exceptionFilters[0].WhenForAnyArgs(x => x.FilterExceptionAsync(command, e)).Do(ci => exceptionFilterContext = TaskContext.Current);

            var beforeContext = TaskContext.Current;
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.HandleAsync(command, CancellationToken.None));
            var afterContext = TaskContext.Current;

            beforeContext.Should().BeSameAs(afterContext);
            exceptionFilterConstructContext.Should().BeSameAs(handlerContext);
            exceptionFilterContext.Should().BeSameAs(handlerContext);
        }

        public class MyCommand : ICommand
        {
        }
    }
}
