using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Revo.AspNetCore.Configuration;
using Revo.Core.Commands;
using Revo.Core.Configuration;
using Revo.Core.Events;
using Revo.Core.Services;
using Revo.Domain.Events;
using Revo.EasyNetQ.Configuration;
using Revo.EFCore.Configuration;
using Revo.EFCore.DataAccess.Configuration;
using Revo.EFCore.Projections;
using Revo.Hangfire;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Jobs;
using Revo.Infrastructure.Sagas;
using Revo.Infrastructure.Security.Commands;
using Revo.Infrastructure.Tenancy;
using Revo.Infrastructure.Validation;
using Xunit;

namespace Revo.IntegrationTests.Container
{
    /// <summary>
    /// Builds the container of an ASP.NET Core application with the Revo configurations and validates it
    /// (all registrations resolvable, no scoped services captured by singletons), then checks that the framework's
    /// services registered for base types (relying on variance) get resolved for concrete commands/events/jobs.
    /// </summary>
    public class ContainerConfigurationTests
    {
        [Fact]
        public async Task DefaultConfiguration_PassesContainerValidation()
        {
            await using var app = BuildApp(revo => revo.ConfigureDefaults());
        }

        [Fact]
        public async Task AspNetCoreWithEFCoreInfrastructure_PassesContainerValidation()
        {
            await using var app = BuildApp(ConfigureAspNetCoreWithEFCoreInfrastructure);
        }

        [Fact]
        public async Task WithEasyNetQ_PassesContainerValidation()
        {
            await using var app = BuildApp(revo =>
            {
                ConfigureAspNetCoreWithEFCoreInfrastructure(revo);
                revo.UseEasyNetQ(connection: new EasyNetQConnectionConfiguration("host=localhost"));
            });
        }

        [Fact]
        public async Task WithHangfire_PassesContainerValidation()
        {
            await using var app = BuildApp(revo =>
            {
                ConfigureAspNetCoreWithEFCoreInfrastructure(revo);
                revo.UseHangfire();
            });
        }

        [Fact]
        public async Task CommandMiddlewaresOfBaseCommandTypesAreResolved()
        {
            await using var app = BuildApp(ConfigureAspNetCoreWithEFCoreInfrastructure);
            await using var scope = app.Services.CreateAsyncScope();

            var middlewares = scope.ServiceProvider.GetRequiredService<ICommandBusMiddlewareFactory>()
                .CreateMiddlewares<TestCommand>(scope.ServiceProvider.GetRequiredService<ICommandBus>());

            middlewares.Select(x => x.GetType()).Should().BeEquivalentTo(new[]
            {
                typeof(TenantContextCommandBusMiddleware),
                typeof(UnitOfWorkCommandBusMiddleware),
                typeof(FilterCommandBusMiddleware<TestCommand>)
            });
        }

        [Fact]
        public async Task PreCommandFiltersOfBaseCommandTypesAreResolved()
        {
            await using var app = BuildApp(ConfigureAspNetCoreWithEFCoreInfrastructure);
            await using var scope = app.Services.CreateAsyncScope();

            var preFilters = scope.ServiceProvider.GetRequiredService<IServiceFactory<IPreCommandFilter<TestCommand>>>()
                .GetServices();

            preFilters.Select(x => x.GetType()).Should().BeEquivalentTo(new[]
            {
                typeof(CommandPermissionAuthorizer),
                typeof(CommandAttributeValidationFilter)
            });
        }

        [Fact]
        public async Task EventListenersOfBaseEventTypesAreResolved()
        {
            await using var app = BuildApp(ConfigureAspNetCoreWithEFCoreInfrastructure);
            await using var scope = app.Services.CreateAsyncScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IVariantServiceResolver>();

            resolver.GetServices(typeof(IEventListener<TestAggregateEvent>)).Select(x => x.GetType())
                .Should().BeEquivalentTo(new[] { typeof(AsyncEventQueueDispatchListener) });

            resolver.GetServices(typeof(IAsyncEventSequencer<TestAggregateEvent>)).Select(x => x.GetType())
                .Should().BeEquivalentTo(new[]
                {
                    typeof(EFCoreProjectionEventListener.EFCoreProjectionEventSequencer),
                    typeof(SagaEventListener.SagaEventSequencer)
                });

            resolver.GetServices(typeof(IAsyncEventListener<TestAggregateEvent>)).Select(x => x.GetType())
                .Should().BeEquivalentTo(new[]
                {
                    typeof(EFCoreProjectionEventListener),
                    typeof(SagaEventListener)
                });
        }

        [Fact]
        public async Task JobHandlersOfBaseJobTypesAreResolved()
        {
            await using var app = BuildApp(ConfigureAspNetCoreWithEFCoreInfrastructure);
            await using var scope = app.Services.CreateAsyncScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IVariantServiceResolver>();

            resolver.GetRequiredService(typeof(IJobHandler<ExecuteCommandJob<TestCommand>>))
                .Should().BeOfType<ExecuteCommandJobHandler>();
        }

        private static void ConfigureAspNetCoreWithEFCoreInfrastructure(IRevoConfiguration revo)
        {
            // explicit assemblies instead of UseAllAppDomainAssembliesForTypeDiscovery, which would make the result
            // depend on which assemblies happen to be loaded already
            revo
                .ConfigureDefaults()
                .ConfigureCore(core => core.AddAssembly(
                    typeof(IRevoConfiguration).Assembly,
                    typeof(DomainAggregateEvent).Assembly,
                    typeof(SagaEventListener).Assembly,
                    typeof(EFCoreProjectionEventListener).Assembly,
                    typeof(AspNetCoreConfigurationExtensions).Assembly,
                    typeof(ContainerConfigurationTests).Assembly))
                .UseAspNetCore()
                .UseEFCoreDataAccess(contextBuilder => contextBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString()))
                .UseEFCoreInfrastructure();
        }

        private static WebApplication BuildApp(Action<IRevoConfiguration> configureRevo)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Host.UseDefaultServiceProvider(options =>
            {
                options.ValidateOnBuild = true;
                options.ValidateScopes = true;
            });

            builder.Services.AddRevo(configureRevo);
            return builder.Build();
        }

        public class TestCommand : ICommand
        {
        }

        public class TestAggregateEvent : DomainAggregateEvent
        {
        }
    }
}
