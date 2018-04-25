using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Commands;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class CommandHandlerPipelineTests_Queries
    {
        private readonly ICommandHandler<MyQuery, int> sut;
        private readonly List<IPreCommandFilter<MyQuery>> preFilters = new List<IPreCommandFilter<MyQuery>>();
        private readonly List<IPostCommandFilter<MyQuery>> postFilters = new List<IPostCommandFilter<MyQuery>>();
        private readonly List<IExceptionCommandFilter<MyQuery>> exceptionFilters = new List<IExceptionCommandFilter<MyQuery>>();
        private readonly ICommandHandler<MyQuery, int> decorated;
        private readonly Func<ICommandHandler<MyQuery, int>> decoratedFunc;

        public CommandHandlerPipelineTests_Queries()
        {
            var preFilter = Substitute.For<IPreCommandFilter<MyQuery>>();
            preFilters.Add(preFilter);

            var postFilter = Substitute.For<IPostCommandFilter<MyQuery>>();
            postFilters.Add(postFilter);

            var exceptionFilter = Substitute.For<IExceptionCommandFilter<MyQuery>>();
            exceptionFilters.Add(exceptionFilter);

            decorated = Substitute.For<ICommandHandler<MyQuery, int>>();
            decoratedFunc = Substitute.For<Func<ICommandHandler<MyQuery, int>>>();
            decoratedFunc().Returns(decorated);

            sut = new CommandHandlerPipeline<MyQuery, int>(preFilters.ToArray(),
                postFilters.ToArray(), exceptionFilters.ToArray(), decoratedFunc);
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

        public class MyQuery : IQuery<int>
        {
        }
    }
}
