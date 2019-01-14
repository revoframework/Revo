using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
