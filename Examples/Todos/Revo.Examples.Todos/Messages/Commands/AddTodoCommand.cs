using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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
