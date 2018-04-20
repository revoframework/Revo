using Revo.Domain.Sagas;

namespace Revo.Infrastructure.Sagas
{
    public class ConventionSagaConfigurator : ISagaConfigurator
    {
        private readonly ISagaConventionConfigurationCache sagaConventionConfigurationCache;

        public ConventionSagaConfigurator(ISagaConventionConfigurationCache sagaConventionConfigurationCache)
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
                            sagaRegistry.Add(SagaEventRegistration.AlwaysStarting(sagaConfigInfo.Key, typeToEventInfos.Key));
                        }
                        else if (eventInfo.SagaKey != null)
                        {
                            sagaRegistry.Add(SagaEventRegistration.MatchedByKey(sagaConfigInfo.Key, typeToEventInfos.Key,
                                eventInfo.EventKeyExpression, eventInfo.SagaKey, eventInfo.IsStartingIfSagaNotFound));
                        }
                        else
                        {
                            sagaRegistry.Add(SagaEventRegistration.ToAllExistingInstances(sagaConfigInfo.Key, typeToEventInfos.Key, eventInfo.IsStartingIfSagaNotFound));
                        }
                    }
                }
            }
        }
    }
}
