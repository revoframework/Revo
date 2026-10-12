using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Revo.AspNetCore.Configuration;
using Revo.AspNetCore.Core;
using Revo.AspNetCore.Security;
using Revo.Core.Configuration;
using Revo.Core.Lifecycle;
using Revo.Core.Security;
using Xunit;

namespace Revo.AspNetCore.Tests.Configuration
{
    public class AspNetCoreConfigurationExtensionsTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UseAspNetCore_OverridesDefaultImplementations(bool useAspNetCoreFirst)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddRevo(cfg =>
            {
                if (useAspNetCoreFirst)
                {
                    cfg.UseAspNetCore().ConfigureDefaults();
                }
                else
                {
                    cfg.ConfigureDefaults().UseAspNetCore();
                }
            });

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            scope.ServiceProvider.GetRequiredService<IUserContext>().Should().BeOfType<AspNetCoreUserContext>();
            scope.ServiceProvider.GetRequiredService<IRequestLifetime>().Should().BeOfType<AspNetCoreRequestLifetime>();
        }

        [Fact]
        public void WithoutAspNetCore_UsesNullImplementations()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddRevo(cfg => cfg.ConfigureDefaults());

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            scope.ServiceProvider.GetRequiredService<IUserContext>().Should().BeOfType<NullUserContext>();
            scope.ServiceProvider.GetRequiredService<IRequestLifetime>().Should().BeOfType<NullRequestLifetime>();
        }
    }
}
