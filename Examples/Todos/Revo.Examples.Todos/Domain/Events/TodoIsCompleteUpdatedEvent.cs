using System;
using Revo.Domain.Events;

namespace Revo.Examples.Todos.Domain.Events
{
    public class TodoIsCompleteUpdatedEvent : DomainAggregateEvent
    {
        public TodoIsCompleteUpdatedEvent(Guid todoId, bool isComplete)
        {
            TodoId = todoId;
            IsComplete = isComplete;
        }

        public Guid TodoId { get; }
        public bool IsComplete { get; }
    }
}
