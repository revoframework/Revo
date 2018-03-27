using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Revo.Core.Events;
using Revo.Domain.Events;
using Revo.Domain.Sagas;
using Revo.Infrastructure.EventSourcing;

namespace Revo.Infrastructure.Sagas
{
    public class SagaLocator : ISagaLocator
    {
        private readonly ISagaRegistry sagaRegistry;
        private readonly ISagaRepository sagaRepository;
        private readonly IEventSourcedEntityFactory eventSourcedEntityFactory = new EventSourcedEntityFactory();

        public SagaLocator(ISagaRegistry sagaRegistry, ISagaRepository sagaRepository)
        {
            this.sagaRegistry = sagaRegistry;
            this.sagaRepository = sagaRepository;
        }

        public async Task LocateAndDispatchAsync(IEnumerable<IEventMessage<DomainEvent>> domainEvents)
        {
            Dictionary<Guid, ISaga> sagas = new Dictionary<Guid, ISaga>();
            List<ISaga> newSagas = new List<ISaga>();

            // TODO optimize: load all sagas at once

            foreach (var domainEvent in domainEvents)
            {
                var registrations = sagaRegistry.LookupRegistrations(domainEvent.Event.GetType());
                foreach (SagaEventRegistration registration in registrations)
                {
                    HashSet<ISaga> matchingSagas = new HashSet<ISaga>();
                    if (!registration.IsAlwaysStarting)
                    {
                        string sagaKeyValue = registration.EventKeyExpression(domainEvent.Event);
                        Guid[] matchingSagaIds = await sagaRepository.MetadataRepository.FindSagaIdsByKeyAsync(
                            registration.SagaKey,
                            sagaKeyValue);
                        foreach (Guid sagaId in matchingSagaIds)
                        {
                            if (!sagas.TryGetValue(sagaId, out var saga))
                            {
                                saga = await sagaRepository.GetAsync(sagaId);
                                sagas.Add(sagaId, saga);
                            }

                            matchingSagas.Add(saga);
                        }

                        newSagas.Where(x => x.Keys.TryGetValue(registration.SagaKey, out var keyValues) && keyValues.Contains(sagaKeyValue))
                            .ForEach(x => matchingSagas.Add(x)); //add new sagas that don't have metadata saved yet
                    }

                    if (registration.IsAlwaysStarting || (matchingSagas.Count == 0 && registration.IsStartingIfSagaNotFound))
                    {
                        ISaga saga = (ISaga) eventSourcedEntityFactory.ConstructEntity(registration.SagaType, Guid.NewGuid());
                        matchingSagas.Add(saga);
                        sagas.Add(saga.Id, saga);
                        sagaRepository.Add(saga);
                        newSagas.Add(saga);
                    }

                    foreach (ISaga saga in matchingSagas)
                    {
                        saga.HandleEvent(domainEvent);
                    }
                }
            }

            if (sagas.Any())
            {
                await sagaRepository.SaveChangesAsync();
            }
        }
    }
}
