using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain;

namespace GTRevo.Infrastructure.Sagas
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
                foreach (var eventInfo in sagaConfigInfo.Value.Events)
                {
                    if (eventInfo.Value.IsAlwaysStarting)
                    {
                        sagaRegistry.Add(new SagaEventRegistration(sagaConfigInfo.Key, eventInfo.Key));
                    }
                    else
                    {
                        sagaRegistry.Add(new SagaEventRegistration(sagaConfigInfo.Key, eventInfo.Key,
                            eventInfo.Value.EventKeyExpression, eventInfo.Value.SagaKey, eventInfo.Value.IsStartingIfSagaNotFound));
                    }
                }
            }
        }
    }
}
