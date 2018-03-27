using Revo.Domain.Sagas;

namespace Revo.Infrastructure.Sagas
{
    public class SagaConventionConfigurator : ISagaConfigurator
    {
        private readonly ISagaConventionConfigurationCache sagaConventionConfigurationCache;

        public SagaConventionConfigurator(ISagaConventionConfigurationCache sagaConventionConfigurationCache)
        {
            this.sagaConventionConfigurationCache = sagaConventionConfigurationCache;
        }

        public void ConfigureSagas(ISagaRegistry sagaRegistry)
        {
            foreach (var sagaConfigInfo in sagaConventionConfigurationCache.ConfigurationInfos)
            {
                foreach (var typeToEventInfos in sagaConfigInfo.Value.Events)
                {
                    foreach (var eventInfo in typeToEventInfos.Value)
                    {
                        if (eventInfo.IsAlwaysStarting)
                        {
                            sagaRegistry.Add(new SagaEventRegistration(sagaConfigInfo.Key, typeToEventInfos.Key));
                        }
                        else
                        {
                            sagaRegistry.Add(new SagaEventRegistration(sagaConfigInfo.Key, typeToEventInfos.Key,
                                eventInfo.EventKeyExpression, eventInfo.SagaKey, eventInfo.IsStartingIfSagaNotFound));
                        }
                    }
                }
            }
        }
    }
}
