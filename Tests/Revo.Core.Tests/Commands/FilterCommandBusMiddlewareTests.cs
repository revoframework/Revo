using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Revo.Core.Commands;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class FilterCommandBusMiddlewareTests
    {
        private readonly FilterCommandBusMiddleware<MyCommand> sut;
        private readonly List<IPreCommandFilter<MyCommand>> preFilters = new List<IPreCommandFilter<MyCommand>>();
        private readonly Func<IPreCommandFilter<MyCommand>[]> preFiltersFunc;
        private readonly List<IPostCommandFilter<MyCommand>> postFilters = new List<IPostCommandFilter<MyCommand>>();
        private readonly Func<IPostCommandFilter<MyCommand>[]> postFiltersFunc;
        private readonly List<IExceptionCommandFilter<MyCommand>> exceptionFilters = new List<IExceptionCommandFilter<MyCommand>>();
        private readonly Func<IExceptionCommandFilter<MyCommand>[]> exceptionFiltersFunc;

        public FilterCommandBusMiddlewareTests()
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
            
            sut = new FilterCommandBusMiddleware<MyCommand>(preFiltersFunc, postFiltersFunc,
                exceptionFiltersFunc);
        }

        [Fact]
        public async Task Handle_CallsPreFilters()
        {
            var command = new MyCommand();
            var next = Substitute.For<CommandBusMiddlewareDelegate>();

            await sut.HandleAsync(command, CommandExecutionOptions.Default, next, CancellationToken.None);

            Received.InOrder(() =>
            {
                preFilters[0].PreFilterAsync(command);
                next.Invoke(command);
            });
        }

        [Fact]
        public async Task Handle_DoesntCallDecoratedWhenPreFilterThrows()
        {
            var command = new MyCommand();
            var next = Substitute.For<CommandBusMiddlewareDelegate>();

            preFilters[0].PreFilterAsync(command).Throws(new Exception());
            
            await Assert.ThrowsAsync<Exception>(async () =>
                await sut.HandleAsync(command, CommandExecutionOptions.Default, next, CancellationToken.None));

            next.DidNotReceiveWithAnyArgs().Invoke(null);
        }

        [Fact]
        public async Task Handle_CallsPostFilters()
        {
            var command = new MyCommand();
            var next = Substitute.For<CommandBusMiddlewareDelegate>();

            await sut.HandleAsync(command, CommandExecutionOptions.Default, next, CancellationToken.None);
            
            Received.InOrder(() =>
            {
                next.Invoke(command);
                postFilters[0].PostFilterAsync(command, null);
            });
        }

        [Fact]
        public async Task Handle_CallsExceptionFilters()
        {
            var command = new MyCommand();
            var next = Substitute.For<CommandBusMiddlewareDelegate>();

            InvalidOperationException e = new InvalidOperationException();
            next.Invoke(command).Throws(e);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await sut.HandleAsync(command, CommandExecutionOptions.Default, next, CancellationToken.None));

            next.Received(1).Invoke(command);
            exceptionFilters[0].Received(1).FilterExceptionAsync(command, e);
            postFilters[0].DidNotReceiveWithAnyArgs().PostFilterAsync(null, null);
        }

        public class MyCommand : ICommand
        {
        }
    }
}