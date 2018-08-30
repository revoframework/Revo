using System;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Sagas
{
    public class SagaEventRegistration
    {
        private SagaEventRegistration(Type sagaType, Type eventType, Func<DomainEvent, string> eventKeyExpression,
            string sagaKey, bool isAlwaysStarting, bool isStartingIfSagaNotFound)
        {
            SagaType = sagaType;
            EventType = eventType;
            EventKeyExpression = eventKeyExpression;
            SagaKey = sagaKey;
            IsAlwaysStarting = isAlwaysStarting;
            IsStartingIfSagaNotFound = isStartingIfSagaNotFound;
        }
        
        public Type SagaType { get; }
        public Type EventType { get; }
        public Func<DomainEvent, string> EventKeyExpression { get; }
        public string SagaKey { get; }
        public bool IsAlwaysStarting { get; }
        public bool IsStartingIfSagaNotFound { get; }
        
        public static SagaEventRegistration MatchedByKey(Type sagaType, Type eventType,
            Func<DomainEvent, string> eventKeyExpression, string sagaKey, bool isStartingIfSagaNotFound)
        {
            return new SagaEventRegistration(sagaType, eventType, eventKeyExpression, sagaKey, false, isStartingIfSagaNotFound);
        }

        public static SagaEventRegistration AlwaysStarting(Type sagaType, Type eventType)
        {
            return new SagaEventRegistration(sagaType, eventType, null, null, true, false);
        }

        public static SagaEventRegistration ToAllExistingInstances(Type sagaType, Type eventType, bool isStartingIfSagaNotFound)
        {
            return new SagaEventRegistration(sagaType, eventType, null, null, false, isStartingIfSagaNotFound);
        }
    }
}
