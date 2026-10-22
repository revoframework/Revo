using System.Threading.Tasks;
using Revo.Core.Configuration;
using Revo.EasyNetQ.Configuration;
using Revo.Hangfire;
using Revo.IntegrationTests.Fixtures;
using Xunit;

namespace Revo.IntegrationTests.Scenarios.Container
{
    /// <summary>
    /// Validates the service providers of applications with various Revo configurations (incl. the providers).
    /// </summary>
    public class ContainerValidationTests
    {
        [Fact]
        public async Task DefaultConfiguration_PassesContainerValidation()
        {
            await using var app = ValidatedAppBuilder.Build(revo => revo.ConfigureDefaults());
        }

        [Fact]
        public async Task AspNetCoreWithEFCoreInfrastructure_PassesContainerValidation()
        {
            await using var app = ValidatedAppBuilder.Build(ValidatedAppBuilder.ConfigureAspNetCoreWithEFCoreInfrastructure);
        }

        [Fact]
        public async Task WithEasyNetQ_PassesContainerValidation()
        {
            await using var app = ValidatedAppBuilder.Build(revo =>
            {
                ValidatedAppBuilder.ConfigureAspNetCoreWithEFCoreInfrastructure(revo);
                revo.UseEasyNetQ(connection: new EasyNetQConnectionConfiguration("host=localhost"));
            });
        }

        [Fact]
        public async Task WithHangfire_PassesContainerValidation()
        {
            await using var app = ValidatedAppBuilder.Build(revo =>
            {
                ValidatedAppBuilder.ConfigureAspNetCoreWithEFCoreInfrastructure(revo);
                revo.UseHangfire();
            });
        }
    }
}
