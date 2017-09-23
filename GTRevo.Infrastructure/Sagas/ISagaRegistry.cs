using System;
using System.Collections.Generic;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Sagas
{
    public interface ISagaRegistry
    {
        void Add(SagaEventRegistration sagaEventRegistration);
        IEnumerable<SagaEventRegistration> LookupRegistrations(Type domainEventType);
    }
}