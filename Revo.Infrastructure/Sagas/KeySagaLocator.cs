using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Domain.Entities;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Sagas
{
    public class KeySagaLocator : ISagaLocator
    {
        private readonly ISagaRegistry sagaRegistry;
        private readonly ISagaMetadataRepository sagaMetadataRepository;

        public KeySagaLocator(ISagaRegistry sagaRegistry, ISagaMetadataRepository sagaMetadataRepository)
        {
            this.sagaRegistry = sagaRegistry;
            this.sagaMetadataRepository = sagaMetadataRepository;
        }

        public async Task<IEnumerable<LocatedSaga>> LocateSagasAsync(IEventMessage<DomainEvent> domainEvent)
        {
            HashSet<LocatedSaga> locatedSagas = new HashSet<LocatedSaga>();

            var registrations = sagaRegistry.LookupRegistrations(domainEvent.Event.GetType());
            foreach (SagaEventRegistration registration in registrations)
            {
                SagaMatch[] sagaMatches = {};
                if (!registration.IsAlwaysStarting)
                {
                    Guid sagaClassId = EntityClassUtils.GetEntityClassId(registration.SagaType);

                    if (registration.EventKeyExpression != null && registration.SagaKey != null)
                    {
                        string sagaKeyValue = registration.EventKeyExpression(domainEvent.Event);
                        sagaMatches = await sagaMetadataRepository.FindSagasByKeyAsync(
                            sagaClassId, registration.SagaKey, sagaKeyValue);
                    }
                    else
                    {
                        sagaMatches = await sagaMetadataRepository.FindSagasAsync(sagaClassId);
                    }

                    foreach (SagaMatch sagaMatch in sagaMatches)
                    {
                        Debug.Assert(sagaMatch.ClassId == sagaClassId);

                        locatedSagas.Add(LocatedSaga.FromId(sagaMatch.Id, registration.SagaType));
                    }
                }

                if (registration.IsAlwaysStarting || (sagaMatches.Length == 0 && registration.IsStartingIfSagaNotFound))
                {
                    locatedSagas.Add(LocatedSaga.CreateNew(registration.SagaType));
                }
            }

            return locatedSagas;
        }
    }
}
