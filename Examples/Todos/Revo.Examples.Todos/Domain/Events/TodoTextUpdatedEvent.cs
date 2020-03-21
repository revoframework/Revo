using System;
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
