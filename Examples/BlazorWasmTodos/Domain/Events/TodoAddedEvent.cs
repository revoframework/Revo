using Revo.Domain.Events;

namespace Revo.Examples.BlazorWasmTodos.Domain.Events
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
