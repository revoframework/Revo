using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Revo.Core.Commands;
using Revo.Core.Services;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class FilterCommandBusMiddlewareTests
    {
        private readonly FilterCommandBusMiddleware<MyCommand> sut;
        private readonly List<IPreCommandFilter<MyCommand>> preFilters = new List<IPreCommandFilter<MyCommand>>();
        private readonly IServiceFactory<IPreCommandFilter<MyCommand>> preFiltersFactory;
        private readonly List<IPostCommandFilter<MyCommand>> postFilters = new List<IPostCommandFilter<MyCommand>>();
        private readonly IServiceFactory<IPostCommandFilter<MyCommand>> postFiltersFactory;
        private readonly List<IExceptionCommandFilter<MyCommand>> exceptionFilters = new List<IExceptionCommandFilter<MyCommand>>();
        private readonly IServiceFactory<IExceptionCommandFilter<MyCommand>> exceptionFiltersFactory;

        public FilterCommandBusMiddlewareTests()
        {
            var preFilter = Substitute.For<IPreCommandFilter<MyCommand>>();
            preFilters.Add(preFilter);
            preFiltersFactory = Substitute.For<IServiceFactory<IPreCommandFilter<MyCommand>>>();
            preFiltersFactory.GetServices().Returns(preFilters);

            var postFilter = Substitute.For<IPostCommandFilter<MyCommand>>();
            postFilters.Add(postFilter);
            postFiltersFactory = Substitute.For<IServiceFactory<IPostCommandFilter<MyCommand>>>();
            postFiltersFactory.GetServices().Returns(postFilters);

            var exceptionFilter = Substitute.For<IExceptionCommandFilter<MyCommand>>();
            exceptionFilters.Add(exceptionFilter);
            exceptionFiltersFactory = Substitute.For<IServiceFactory<IExceptionCommandFilter<MyCommand>>>();
            exceptionFiltersFactory.GetServices().Returns(exceptionFilters);
            
            sut = new FilterCommandBusMiddleware<MyCommand>(preFiltersFactory, postFiltersFactory,
                exceptionFiltersFactory);
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
                next.Invoke(command, CancellationToken.None);
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

            next.DidNotReceiveWithAnyArgs().Invoke(null, CancellationToken.None);
        }

        [Fact]
        public async Task Handle_CallsPostFilters()
        {
            var command = new MyCommand();
            var next = Substitute.For<CommandBusMiddlewareDelegate>();

            await sut.HandleAsync(command, CommandExecutionOptions.Default, next, CancellationToken.None);
            
            Received.InOrder(() =>
            {
                next.Invoke(command, CancellationToken.None);
                postFilters[0].PostFilterAsync(command, null);
            });
        }

        [Fact]
        public async Task Handle_CallsExceptionFilters()
        {
            var command = new MyCommand();
            var next = Substitute.For<CommandBusMiddlewareDelegate>();

            InvalidOperationException e = new InvalidOperationException();
            next.Invoke(command, CancellationToken.None).Throws(e);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await sut.HandleAsync(command, CommandExecutionOptions.Default, next, CancellationToken.None));

            next.Received(1).Invoke(command, CancellationToken.None);
            exceptionFilters[0].Received(1).FilterExceptionAsync(command, e);
            postFilters[0].DidNotReceiveWithAnyArgs().PostFilterAsync(null, null);
        }

        [Fact]
        public async Task Handle_CallsFiltersOfBaseCommandTypes()
        {
            var preFilter = Substitute.For<IPreCommandFilter<ICommandBase>>();
            var postFilter = Substitute.For<IPostCommandFilter<ICommandBase>>();
            var services = new ServiceCollection();
            services.AddSingleton(preFilter);
            services.AddSingleton(postFilter);

            var resolver = new VariantServiceResolver(services.BuildServiceProvider(),
                new VariantServiceTypeIndex(services));
            var middleware = new FilterCommandBusMiddleware<MyCommand>(
                new ContainerServiceFactory<IPreCommandFilter<MyCommand>>(resolver),
                new ContainerServiceFactory<IPostCommandFilter<MyCommand>>(resolver),
                new ContainerServiceFactory<IExceptionCommandFilter<MyCommand>>(resolver));

            var command = new MyCommand();
            var next = Substitute.For<CommandBusMiddlewareDelegate>();

            await middleware.HandleAsync(command, CommandExecutionOptions.Default, next, CancellationToken.None);

            await preFilter.Received(1).PreFilterAsync(command);
            await postFilter.Received(1).PostFilterAsync(command, null);
        }

        public class MyCommand : ICommand
        {
        }
    }
}