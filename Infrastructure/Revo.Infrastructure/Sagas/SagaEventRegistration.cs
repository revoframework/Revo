using System;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Sagas
{
    public struct SagaEventRegistration
    {
        public SagaEventRegistration(Type sagaType, Type eventType, Func<DomainEvent, string> eventKeyExpression,
            string sagaKey, bool isStartingIfSagaNotFound)
        {
            SagaType = sagaType;
            EventType = eventType;
            EventKeyExpression = eventKeyExpression;
            SagaKey = sagaKey;
            IsAlwaysStarting = false;
            IsStartingIfSagaNotFound = isStartingIfSagaNotFound;
        }

        public SagaEventRegistration(Type sagaType, Type eventType)
        {
            SagaType = sagaType;
            EventType = eventType;
            EventKeyExpression = null;
            SagaKey = null;
            IsAlwaysStarting = true;
            IsStartingIfSagaNotFound = false;
        }

        public Type SagaType { get; }
        public Type EventType { get; }
        public Func<DomainEvent, string> EventKeyExpression { get; }
        public string SagaKey { get; }
        public bool IsAlwaysStarting { get; }
        public bool IsStartingIfSagaNotFound { get; }
    }
}
