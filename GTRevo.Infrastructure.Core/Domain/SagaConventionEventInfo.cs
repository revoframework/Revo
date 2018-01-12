using System;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Core.Domain
{
    public class SagaConventionEventInfo
    {
        public SagaConventionEventInfo(Func<DomainEvent, string> eventKeyExpression,
            string sagaKey, bool isStartingIfSagaNotFound, Action<Saga, IEventMessage<DomainEvent>> handleDelegate)
        {
            HandleDelegate = handleDelegate;
            EventKeyExpression = eventKeyExpression;
            SagaKey = sagaKey;
            IsAlwaysStarting = false;
            IsStartingIfSagaNotFound = isStartingIfSagaNotFound;
        }

        public SagaConventionEventInfo(Action<Saga, IEventMessage<DomainEvent>> handleDelegate)
        {
            HandleDelegate = handleDelegate;
            IsAlwaysStarting = true;
            IsStartingIfSagaNotFound = false;
        }

        public bool IsAlwaysStarting { get; }
        public bool IsStartingIfSagaNotFound { get; }
        public string SagaKey { get; }
        public Func<DomainEvent, string> EventKeyExpression { get; }
        public Action<Saga, IEventMessage<DomainEvent>> HandleDelegate { get; }
    }
}
