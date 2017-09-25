using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Sagas
{
    public class SagaRegistry : ISagaRegistry
    {
        private readonly MultiValueDictionary<Type, SagaEventRegistration> eventTypeRegistrations =
            new MultiValueDictionary<Type, SagaEventRegistration>();

        public void Add(SagaEventRegistration sagaEventRegistration)
        {
            eventTypeRegistrations.Add(sagaEventRegistration.EventType, sagaEventRegistration);
        }

        public IEnumerable<SagaEventRegistration> LookupRegistrations(Type domainEventType)
        {
            if (!typeof(DomainEvent).IsAssignableFrom(domainEventType))
            {
                throw new ArgumentException(
                    $"Cannot lookup saga event registrations for event type: {domainEventType.FullName} because it's not a DomainEvent");
            }

            if (eventTypeRegistrations.TryGetValue(domainEventType,
                out IReadOnlyCollection<SagaEventRegistration> registrations))
            {
                return registrations;
            }

            return Enumerable.Empty<SagaEventRegistration>();
        }
    }
}
