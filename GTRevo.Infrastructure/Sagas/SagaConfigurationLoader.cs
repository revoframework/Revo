using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core.Lifecycle;

namespace GTRevo.Infrastructure.Sagas
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
