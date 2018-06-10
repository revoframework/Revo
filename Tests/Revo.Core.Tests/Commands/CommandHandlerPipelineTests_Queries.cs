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
    public class CommandHandlerPipelineTests_Queries
    {
        private readonly ICommandHandler<MyQuery, int> sut;
        private readonly List<IPreCommandFilter<MyQuery>> preFilters = new List<IPreCommandFilter<MyQuery>>();
        private readonly Func<IPreCommandFilter<MyQuery>[]> preFiltersFunc;
        private readonly List<IPostCommandFilter<MyQuery>> postFilters = new List<IPostCommandFilter<MyQuery>>();
        private readonly Func<IPostCommandFilter<MyQuery>[]> postFiltersFunc;
        private readonly List<IExceptionCommandFilter<MyQuery>> exceptionFilters = new List<IExceptionCommandFilter<MyQuery>>();
        private readonly Func<IExceptionCommandFilter<MyQuery>[]> exceptionFiltersFunc;
        private readonly ICommandHandler<MyQuery, int> decorated;
        private readonly Func<ICommandHandler<MyQuery, int>> decoratedFunc;

        public CommandHandlerPipelineTests_Queries()
        {
            var preFilter = Substitute.For<IPreCommandFilter<MyQuery>>();
            preFilters.Add(preFilter);
            preFiltersFunc = Substitute.For<Func<IPreCommandFilter<MyQuery>[]>>();
            preFiltersFunc.Invoke().Returns(preFilters.ToArray());

            var postFilter = Substitute.For<IPostCommandFilter<MyQuery>>();
            postFilters.Add(postFilter);
            postFiltersFunc = Substitute.For<Func<IPostCommandFilter<MyQuery>[]>>();
            postFiltersFunc.Invoke().Returns(postFilters.ToArray());

            var exceptionFilter = Substitute.For<IExceptionCommandFilter<MyQuery>>();
            exceptionFilters.Add(exceptionFilter);
            exceptionFiltersFunc = Substitute.For<Func<IExceptionCommandFilter<MyQuery>[]>>();
            exceptionFiltersFunc.Invoke().Returns(exceptionFilters.ToArray());

            decorated = Substitute.For<ICommandHandler<MyQuery, int>>();
            decoratedFunc = Substitute.For<Func<ICommandHandler<MyQuery, int>>>();
            decoratedFunc().Returns(decorated);

            sut = new CommandHandlerPipeline<MyQuery, int>(preFiltersFunc, postFiltersFunc,
                exceptionFiltersFunc, decoratedFunc);
        }

        [Fact]
        public async Task Handle_ReturnsResult()
        {
            var command = new MyQuery();
            var cancellationToken = new CancellationToken();
            decorated.HandleAsync(command, cancellationToken).Returns(42);

            int result = await sut.HandleAsync(command, cancellationToken);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_CallsPostFiltersWithResult()
        {
            var command = new MyQuery();
            var cancellationToken = new CancellationToken();
            decorated.HandleAsync(command, cancellationToken).Returns(42);

            await sut.HandleAsync(command, cancellationToken);

            Received.InOrder(()
                =>
            {
                decorated.HandleAsync(command, cancellationToken);
                postFilters[0].PostFilterAsync(command, 42);
            });
        }

        [Fact]
        public async Task Handle_RunsInNewTaskScope()
        {
            var command = new MyQuery();

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
            var command = new MyQuery();

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

        public class MyQuery : IQuery<int>
        {
        }
    }
}
