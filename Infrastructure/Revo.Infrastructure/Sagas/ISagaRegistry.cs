using System;
using System.Collections.Generic;

namespace Revo.Infrastructure.Sagas
{
    public interface ISagaRegistry
    {
        void Add(SagaEventRegistration sagaEventRegistration);
        IEnumerable<SagaEventRegistration> LookupRegistrations(Type domainEventType);
    }
}