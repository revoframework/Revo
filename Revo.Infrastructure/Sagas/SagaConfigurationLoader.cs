using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.Sagas
{
    public class SagaConfigurationLoader(ISagaConfigurator[] sagaConfigurators,
            ISagaRegistry sagaRegistry)
     : IApplicationStartedListener
    {
        public void OnApplicationStarted()
        {
            foreach (ISagaConfigurator sagaConfigurator in sagaConfigurators)
            {
                sagaConfigurator.ConfigureSagas(sagaRegistry);
            }
        }
    }
}
