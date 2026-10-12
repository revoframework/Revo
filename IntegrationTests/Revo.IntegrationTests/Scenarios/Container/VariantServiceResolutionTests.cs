using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Core.Services;
using Revo.EFCore.Projections;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Jobs;
using Revo.Infrastructure.Sagas;
using Revo.Infrastructure.Security.Commands;
using Revo.Infrastructure.Tenancy;
using Revo.Infrastructure.Validation;
using Revo.IntegrationTests.Fixtures;
using Revo.IntegrationTests.TestApp.Domain.Events;
using Revo.IntegrationTests.TestApp.Messages.Commands;
using Xunit;

namespace Revo.IntegrationTests.Scenarios.Container
{
    /// <summary>
    /// Checks that the framework's services registered for base types (relying on generic variance) get resolved
    /// for concrete commands, events and jobs - exactly once each.
    /// </summary>
    public class VariantServiceResolutionTests
    {
        [Fact]
        public async Task CommandMiddlewaresOfBaseCommandTypesAreResolved()
        {
            await using var app = ValidatedAppBuilder.Build(ValidatedAppBuilder.ConfigureAspNetCoreWithEFCoreInfrastructure);
            await using var scope = app.Services.CreateAsyncScope();

            var middlewares = scope.ServiceProvider.GetRequiredService<ICommandBusMiddlewareFactory>()
                .CreateMiddlewares<CreateTestListCommand>(scope.ServiceProvider.GetRequiredService<ICommandBus>());

            middlewares.Select(x => x.GetType()).Should().BeEquivalentTo(new[]
            {
                typeof(TenantContextCommandBusMiddleware),
                typeof(UnitOfWorkCommandBusMiddleware),
                typeof(FilterCommandBusMiddleware<CreateTestListCommand>)
            });
        }

        [Fact]
        public async Task PreCommandFiltersOfBaseCommandTypesAreResolved()
        {
            await using var app = ValidatedAppBuilder.Build(ValidatedAppBuilder.ConfigureAspNetCoreWithEFCoreInfrastructure);
            await using var scope = app.Services.CreateAsyncScope();

            var preFilters = scope.ServiceProvider
                .GetRequiredService<IServiceFactory<IPreCommandFilter<CreateTestListCommand>>>()
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
            await using var app = ValidatedAppBuilder.Build(ValidatedAppBuilder.ConfigureAspNetCoreWithEFCoreInfrastructure);
            await using var scope = app.Services.CreateAsyncScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IVariantServiceResolver>();

            resolver.GetServices(typeof(IEventListener<TestListRenamedEvent>)).Select(x => x.GetType())
                .Should().BeEquivalentTo(new[] { typeof(AsyncEventQueueDispatchListener) });

            resolver.GetServices(typeof(IAsyncEventSequencer<TestListRenamedEvent>)).Select(x => x.GetType())
                .Should().BeEquivalentTo(new[]
                {
                    typeof(EFCoreProjectionEventListener.EFCoreProjectionEventSequencer),
                    typeof(SagaEventListener.SagaEventSequencer)
                });

            resolver.GetServices(typeof(IAsyncEventListener<TestListRenamedEvent>)).Select(x => x.GetType())
                .Should().BeEquivalentTo(new[]
                {
                    typeof(EFCoreProjectionEventListener),
                    typeof(SagaEventListener)
                });
        }

        [Fact]
        public async Task JobHandlersOfBaseJobTypesAreResolved()
        {
            await using var app = ValidatedAppBuilder.Build(ValidatedAppBuilder.ConfigureAspNetCoreWithEFCoreInfrastructure);
            await using var scope = app.Services.CreateAsyncScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IVariantServiceResolver>();

            resolver.GetRequiredService(typeof(IJobHandler<ExecuteCommandJob<CreateTestListCommand>>))
                .Should().BeOfType<ExecuteCommandJobHandler>();
        }
    }
}
