using System;
using System.Collections.Generic;
using Revo.Domain.Sagas;

namespace Revo.Infrastructure.Sagas
{
    public class SagasConfiguration
    {
        private readonly Dictionary<Type, ISagaTypeConfiguration> sagaTypes =
            new Dictionary<Type, ISagaTypeConfiguration>();

        public SagaTypeConfiguration<TSaga> RegisterSaga<TSaga>() where TSaga : ISaga
        {
            if (sagaTypes.TryGetValue(typeof(TSaga), out ISagaTypeConfiguration sagaTypeConfiguration))
            {
                return (SagaTypeConfiguration<TSaga>) sagaTypeConfiguration;
            }

            SagaTypeConfiguration<TSaga> genericSagaTypeConfiguration = new SagaTypeConfiguration<TSaga>();
            sagaTypes[typeof(TSaga)] = genericSagaTypeConfiguration;
            return genericSagaTypeConfiguration;
        }

        public void LoadToRegistry(ISagaRegistry sagaRegistry)
        {
            foreach (var sagaType in sagaTypes)
            {
                foreach (var eventRegistration in sagaType.Value.EventRegistrations)
                {
                    sagaRegistry.Add(eventRegistration);
                }
            }
        }
    }
}
