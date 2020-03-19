using System;
using Revo.Domain.Entities.Attributes;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Examples.Todos.Domain.Events;

namespace Revo.Examples.Todos.Domain
{
    [DomainClassId("D8A1F0C6-CD0A-4F66-8181-336AAFE11248")]
    public class Todo : EventSourcedEntity
    {
        public Todo(Guid id, IAggregateEventRouter eventRouter) : base(id, eventRouter)
        {
        }

        public bool IsComplete { get; private set; }
        public string Text { get; private set; }

        public void UpdateText(string text)
        {
            if (Text != text)
            {
                Publish(new TodoTextUpdatedEvent(Id, text));
            }
        }

        public void MarkComplete(bool isComplete)
        {
            if (IsComplete != isComplete)
            {
                Publish(new TodoIsCompleteUpdatedEvent(Id, isComplete));
            }
        }

        private void Apply(TodoTextUpdatedEvent ev)
        {
            if (ev.TodoId == Id)
            {
                Text = ev.Text;
            }
        }

        private void Apply(TodoIsCompleteUpdatedEvent ev)
        {
            if (ev.TodoId == Id)
            {
                IsComplete = ev.IsComplete;
            }
        }
    }
}
