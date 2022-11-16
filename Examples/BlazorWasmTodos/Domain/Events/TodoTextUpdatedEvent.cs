using Revo.Domain.Events;

namespace Revo.Examples.BlazorWasmTodos.Domain.Events
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
