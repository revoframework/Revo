using Revo.Domain.Events;

namespace Revo.Examples.BlazorWasmTodos.Domain.Events
{
    public class TodoListRenamedEvent : DomainAggregateEvent
    {
        public TodoListRenamedEvent(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
