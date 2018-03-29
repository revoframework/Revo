using System;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Domain.Sagas
{
    public class SagaConventionEventInfo
    {
        public SagaConventionEventInfo(Func<DomainEvent, string> eventKeyExpression,
            string sagaKey, bool isStartingIfSagaNotFound, Action<ISaga, IEventMessage<DomainEvent>> handleDelegate)
        {
            HandleDelegate = handleDelegate;
            EventKeyExpression = eventKeyExpression;
            SagaKey = sagaKey;
            IsAlwaysStarting = false;
            IsStartingIfSagaNotFound = isStartingIfSagaNotFound;
        }

        public SagaConventionEventInfo(Action<ISaga, IEventMessage<DomainEvent>> handleDelegate)
        {
            HandleDelegate = handleDelegate;
            IsAlwaysStarting = true;
            IsStartingIfSagaNotFound = false;
        }

        public bool IsAlwaysStarting { get; }
        public bool IsStartingIfSagaNotFound { get; }
        public string SagaKey { get; }
        public Func<DomainEvent, string> EventKeyExpression { get; }
        public Action<ISaga, IEventMessage<DomainEvent>> HandleDelegate { get; }
    }
}
