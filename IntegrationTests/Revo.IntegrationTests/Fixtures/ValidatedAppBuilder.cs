using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Revo.AspNetCore.Configuration;
using Revo.Core.Configuration;
using Revo.EFCore.Configuration;
using Revo.EFCore.DataAccess.Configuration;
using Revo.IntegrationTests.TestApp;

namespace Revo.IntegrationTests.Fixtures
{
    /// <summary>
    /// Builds (without starting) ASP.NET Core applications with given Revo configurations, validating their service
    /// providers on build (all registrations resolvable, no scoped services captured by singletons).
    /// </summary>
    public static class ValidatedAppBuilder
    {
        public static WebApplication Build(Action<IRevoConfiguration> configureRevo)
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

        public static void ConfigureAspNetCoreWithEFCoreInfrastructure(IRevoConfiguration revo)
        {
            revo
                .ConfigureDefaults()
                .ConfigureCore(core => core.AddAssembly(TestAppConfiguration.DiscoveryAssemblies))
                .UseAspNetCore()
                .UseEFCoreDataAccess(contextBuilder => contextBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString()))
                .UseEFCoreInfrastructure();
        }
    }
}
