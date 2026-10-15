using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Revo.Core.Commands;
using Revo.Core.Services;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class CommandBusMiddlewareFactoryTests
    {
        [Fact]
        public void CreateMiddlewares_IncludesMiddlewaresForBaseCommandTypes()
        {
            var services = new ServiceCollection();
            services.AddTransient<ICommandBusMiddleware<MyCommand>, MyCommandMiddleware>();
            services.AddTransient<ICommandBusMiddleware<ICommandBase>, AnyCommandMiddleware>();
            services.AddTransient<ICommandBusMiddleware<OtherCommand>, OtherCommandMiddleware>();

            var sut = new CommandBusMiddlewareFactory(
                new VariantServiceResolver(services.BuildServiceProvider(), new VariantServiceTypeIndex(services)));

            var middlewares = sut.CreateMiddlewares<MyCommand>(Substitute.For<ICommandBus>());

            middlewares.Select(x => x.GetType()).Should().Equal(
                typeof(MyCommandMiddleware), typeof(AnyCommandMiddleware));
        }

        [Fact]
        public void CreateMiddlewares_CreatesOpenGenericMiddlewaresOnlyForCommandType()
        {
            // like FilterCommandBusMiddleware<>, which must not also run as FilterCommandBusMiddleware<ICommandBase>
            var services = new ServiceCollection();
            services.AddTransient(typeof(ICommandBusMiddleware<>), typeof(GenericMiddleware<>));
            services.AddTransient<ICommandBusMiddleware<ICommandBase>, AnyCommandMiddleware>();

            var sut = new CommandBusMiddlewareFactory(
                new VariantServiceResolver(services.BuildServiceProvider(), new VariantServiceTypeIndex(services)));

            var middlewares = sut.CreateMiddlewares<MyCommand>(Substitute.For<ICommandBus>());

            middlewares.Select(x => x.GetType()).Should().Equal(
                typeof(GenericMiddleware<MyCommand>), typeof(AnyCommandMiddleware));
        }

        public class MyCommand : ICommand
        {
        }

        public class OtherCommand : ICommand
        {
        }

        public class MyCommandMiddleware : TestMiddleware<MyCommand>
        {
        }

        public class AnyCommandMiddleware : TestMiddleware<ICommandBase>
        {
        }

        public class OtherCommandMiddleware : TestMiddleware<OtherCommand>
        {
        }

        public class GenericMiddleware<T> : TestMiddleware<T>
            where T : class, ICommandBase
        {
        }

        public abstract class TestMiddleware<T> : ICommandBusMiddleware<T>
            where T : class, ICommandBase
        {
            public int Order { get; set; }

            public Task<object> HandleAsync(T command, CommandExecutionOptions executionOptions,
                CommandBusMiddlewareDelegate next, CancellationToken cancellationToken)
            {
                return next(command, cancellationToken);
            }
        }
    }
}
