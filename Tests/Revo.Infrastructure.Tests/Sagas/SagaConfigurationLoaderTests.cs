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
        public void OnApplicationStarted_ConfiguresSagas()
        {
            sut.OnApplicationStarted();

            sagaConfigurators[0].Received(1).ConfigureSagas(sagaRegistry);
            sagaConfigurators[1].Received(1).ConfigureSagas(sagaRegistry);
        }
    }
}
