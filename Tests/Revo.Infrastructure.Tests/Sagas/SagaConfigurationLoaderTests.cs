using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Infrastructure.Sagas;
using Xunit;

namespace Revo.Infrastructure.Tests.Sagas
{
    public class SagaConfigurationLoaderTests
    {
        private readonly SagaConfigurationLoader sut;
        private readonly ISagaConfigurator[] sagaConfigurators;
        private readonly ISagaRegistry sagaRegistry;

        public SagaConfigurationLoaderTests()
        {
            sagaConfigurators = new[] {Substitute.For<ISagaConfigurator>(), Substitute.For<ISagaConfigurator>()};
            sagaRegistry = Substitute.For<ISagaRegistry>();

            sut = new SagaConfigurationLoader(sagaConfigurators, sagaRegistry);
        }

        [Fact]
        public async Task ExecuteAsync_ConfiguresSagas()
        {
            await sut.ExecuteAsync(CancellationToken.None);

            sagaConfigurators[0].Received(1).ConfigureSagas(sagaRegistry);
            sagaConfigurators[1].Received(1).ConfigureSagas(sagaRegistry);
        }
    }
}
