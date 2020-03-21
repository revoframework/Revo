using System;
using System.ComponentModel.DataAnnotations;
using Revo.Core.Commands;

namespace Revo.Examples.Todos.Messages.Commands
{
    public class AddTodoCommand : ICommand
    {
        public AddTodoCommand(Guid todoListId, string text)
        {
            TodoListId = todoListId;
            Text = text;
        }

        public Guid TodoListId { get; }

        [Required]
        public string Text { get; }
    }
}
