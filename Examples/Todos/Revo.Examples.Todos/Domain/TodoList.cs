using System;
using System.Collections.Generic;
using Revo.Domain.Entities.Attributes;
using Revo.Domain.Entities.EventSourcing;
using Revo.Examples.Todos.Domain.Events;

namespace Revo.Examples.Todos.Domain
{
    [DomainClassId("9D1C248D-A389-41CC-A93D-3419D7F1CA37")]
    public class TodoList : EventSourcedAggregateRoot
    {
        private Dictionary<Guid, Todo> todos = new Dictionary<Guid, Todo>();

        public TodoList(Guid id, string name) : base(id)
        {
            Rename(name);
        }

        protected TodoList(Guid id) : base(id)
        {
        }

        public string Name { get; private set; }
        public IReadOnlyCollection<Todo> Todos => todos.Values;

        public Todo AddTodo(string text)
        {
            Guid todoId = Guid.NewGuid();
            Publish(new TodoAddedEvent(todoId));

            var todo = todos[todoId];
            todo.UpdateText(text);

            return todo;
        }

        public void Rename(string name)
        {
            if (Name != name)
            {
                Publish(new TodoListRenamedEvent(name));
            }
        }

        private void Apply(TodoAddedEvent ev)
        {
            todos[ev.TodoId] = new Todo(ev.TodoId, EventRouter);
        }

        private void Apply(TodoListRenamedEvent ev)
        {
            Name = ev.Name;
        }
    }
}
