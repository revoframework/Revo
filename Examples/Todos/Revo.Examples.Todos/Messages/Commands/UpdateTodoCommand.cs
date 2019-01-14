using System;
using Revo.Core.Commands;

namespace Revo.Examples.Todos.Messages.Commands
{
    public class UpdateTodoCommand : ICommand
    {
        public UpdateTodoCommand(Guid todoListId, Guid todoId, bool isComplete, string text)
        {
            TodoListId = todoListId;
            TodoId = todoId;
            IsComplete = isComplete;
            Text = text;
        }

        public Guid TodoListId { get; }
        public Guid TodoId { get; }
        public bool IsComplete { get; }
        public string Text { get; }
    }
}
