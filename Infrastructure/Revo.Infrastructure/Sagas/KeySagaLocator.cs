using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Revo.Core.Events;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Revo.Domain.Sagas;
using Revo.Infrastructure.EventSourcing;

namespace Revo.Infrastructure.Sagas
{
    public class KeySagaLocator : ISagaLocator
    {
        private readonly ISagaRegistry sagaRegistry;
        private readonly ISagaMetadataRepository sagaMetadataRepository;
        private readonly IEntityTypeManager entityTypeManager;

        public KeySagaLocator(ISagaRegistry sagaRegistry, ISagaMetadataRepository sagaMetadataRepository,
            IEntityTypeManager entityTypeManager)
        {
            this.sagaRegistry = sagaRegistry;
            this.sagaMetadataRepository = sagaMetadataRepository;
            this.entityTypeManager = entityTypeManager;
        }

        public async Task<IEnumerable<LocatedSaga>> LocateSagasAsync(IEventMessage<DomainEvent> domainEvent)
        {
            List<LocatedSaga> locatedSagas = new List<LocatedSaga>();

            var registrations = sagaRegistry.LookupRegistrations(domainEvent.Event.GetType());
            foreach (SagaEventRegistration registration in registrations)
            {
                SagaKeyMatch[] sagaMatches = {};
                if (!registration.IsAlwaysStarting)
                {
                    string sagaKeyValue = registration.EventKeyExpression(domainEvent.Event);
                    sagaMatches = await sagaMetadataRepository
                        .FindSagasByKeyAsync(registration.SagaKey, sagaKeyValue);
                    foreach (SagaKeyMatch sagaMatch in sagaMatches)
                    {
                        Type sagaType = entityTypeManager.GetClrTypeByClassId(sagaMatch.ClassId);
                        locatedSagas.Add(new LocatedSaga(sagaMatch.Id, sagaType));
                    }
                }

                if (registration.IsAlwaysStarting || (sagaMatches.Length == 0 && registration.IsStartingIfSagaNotFound))
                {
                    locatedSagas.Add(new LocatedSaga(registration.SagaType));
                }
            }

            return locatedSagas;
        }
    }
}
