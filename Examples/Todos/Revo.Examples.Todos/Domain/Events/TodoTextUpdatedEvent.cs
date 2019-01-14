using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Domain.Events;

namespace Revo.Examples.Todos.Domain.Events
{
    public class TodoTextUpdatedEvent : DomainAggregateEvent
    {
        public TodoTextUpdatedEvent(Guid todoId, string text)
        {
            TodoId = todoId;
            Text = text;
        }

        public Guid TodoId { get; }
        public string Text { get; }
    }
}
