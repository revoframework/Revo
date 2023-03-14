using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Lifecycle;
using Revo.Core.Types;

namespace Revo.Domain.Sagas
{
    public class SagaConventionConfigurationCache : ISagaConventionConfigurationCache, IApplicationStartedListener
    {
        private static Dictionary<Type, SagaConfigurationInfo> configurationInfos;

        private readonly ITypeExplorer typeExplorer;

        public SagaConventionConfigurationCache(ITypeExplorer typeExplorer)
        {
            this.typeExplorer = typeExplorer;
            CreateConfigurationInfos();
        }

        public IReadOnlyDictionary<Type, SagaConfigurationInfo> ConfigurationInfos => configurationInfos;

        public void OnApplicationStarted()
        {
            //construction of the object itself is enough
        }

        public static SagaConfigurationInfo GetSagaConfigurationInfo(Type sagaType)
        {
            if (!typeof(IConventionBasedSaga).IsAssignableFrom(sagaType))
            {
                throw new ArgumentException($"{sagaType} is not an IConventionBasedSaga");
            }

            if (configurationInfos == null)
            {
                return SagaConventionConfigurationScanner.GetSagaConfiguration(sagaType);
            }

            SagaConfigurationInfo configurationInfo;
            if (!configurationInfos.TryGetValue(sagaType, out configurationInfo))
            {
                throw new ArgumentException($"Unknown saga type to get convention configuration info for: " + sagaType.FullName);
            }

            return configurationInfo;
        }
        
        private void CreateConfigurationInfos()
        {
            configurationInfos = new Dictionary<Type, SagaConfigurationInfo>();
            var sagaTypes = typeExplorer.GetAllTypes()
                .Where(x => x.IsClass && typeof(IConventionBasedSaga).IsAssignableFrom(x)
                    && !x.IsAbstract && !x.IsGenericTypeDefinition);

            foreach (Type sagaType in sagaTypes)
            {
                configurationInfos.Add(sagaType, SagaConventionConfigurationScanner.GetSagaConfiguration(sagaType));
            }
        }
    }
}
