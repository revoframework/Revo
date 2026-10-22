using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Revo.AspNetCore.Configuration;
using Revo.Core.Configuration;
using Revo.Domain.Events;
using Revo.EFCore.Configuration;
using Revo.EFCore.DataAccess.Configuration;
using Revo.EFCore.DataAccess.Conventions;
using Revo.EFCore.Projections;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Sagas;
using Revo.IntegrationTests.TestApp.Api;
using Revo.IntegrationTests.TestApp.AsyncEvents;

namespace Revo.IntegrationTests.TestApp
{
    /// <summary>
    /// Composition root of the test application (what would be its Program.cs), independent of how it is hosted
    /// and which database it uses.
    /// </summary>
    public static class TestAppConfiguration
    {
        /// <summary>
        /// Assemblies scanned for Revo types (explicitly, instead of UseAllAppDomainAssembliesForTypeDiscovery,
        /// which would make the results depend on which assemblies happen to be loaded already).
        /// </summary>
        public static readonly Assembly[] DiscoveryAssemblies =
        [
            typeof(IRevoConfiguration).Assembly,
            typeof(DomainAggregateEvent).Assembly,
            typeof(SagaEventListener).Assembly,
            typeof(EFCoreProjectionEventListener).Assembly,
            typeof(AspNetCoreConfigurationExtensions).Assembly,
            typeof(TestAppConfiguration).Assembly
        ];

        public static IServiceCollection AddTestApp(this IServiceCollection services,
            Action<DbContextOptionsBuilder> configureDbContext)
        {
            services.AddSingleton<AsyncEventRecorder>();
            services.AddRevo(revo => revo
                .ConfigureDefaults()
                .ConfigureCore(core => core.AddAssembly(DiscoveryAssemblies))
                .UseAspNetCore()
                .UseEFCoreDataAccess(
                    configureDbContext,
                    advancedAction: config => config
                        .AddConvention<BaseTypeAttributeConvention>(-200)
                        .AddConvention<IdColumnsPrefixedWithTableNameConvention>(-110)
                        .AddConvention<PrefixConvention>(-9)
                        .AddConvention<SnakeCaseTableNamesConvention>(1)
                        .AddConvention<SnakeCaseColumnNamesConvention>(1)
                        .AddConvention<LowerCaseConvention>(2))
                .UseEFCoreInfrastructure()
                .ConfigureServices(revoServices =>
                {
                    revoServices.AddSingleton(new ResourceDatabaseMigrationDiscoveryAssembly(
                        typeof(TestAppConfiguration).Assembly, "TestApp/Sql"));
                    revoServices.BindAsyncEventListener<RecordingAsyncEventListener,
                        RecordingAsyncEventListener.Sequencer>();
                }));

            return services;
        }

        public static WebApplication UseTestApp(this WebApplication app)
        {
            app.UseMiddleware<ExceptionStatusCodeMiddleware>();
            app.MapTestAppEndpoints();
            return app;
        }
    }
}
