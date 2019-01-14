using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Domain.Events;

namespace Revo.Examples.Todos.Domain.Events
{
    public class TodoAddedEvent : DomainAggregateEvent
    {
        public TodoAddedEvent(Guid todoId)
        {
            TodoId = todoId;
        }

        public Guid TodoId { get; }
    }
}
