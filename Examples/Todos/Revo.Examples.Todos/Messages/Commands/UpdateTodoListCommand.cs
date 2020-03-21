using System;
using System.ComponentModel.DataAnnotations;
using Revo.Core.Commands;

namespace Revo.Examples.Todos.Messages.Commands
{
    public class UpdateTodoListCommand : ICommand
    {
        public UpdateTodoListCommand(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; }

        [Required]
        public string Name { get; }
    }
}
