using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.Sagas
{
    public class SagaConfigurationLoader : IApplicationStartListener
    {
        private readonly ISagaConfigurator[] sagaConfigurators;
        private readonly ISagaRegistry sagaRegistry;

        public SagaConfigurationLoader(ISagaConfigurator[] sagaConfigurators,
            ISagaRegistry sagaRegistry)
        {
            this.sagaConfigurators = sagaConfigurators;
            this.sagaRegistry = sagaRegistry;
        }

        public void OnApplicationStarted()
        {
            foreach (ISagaConfigurator sagaConfigurator in sagaConfigurators)
            {
                sagaConfigurator.ConfigureSagas(sagaRegistry);
            }
        }
    }
}
