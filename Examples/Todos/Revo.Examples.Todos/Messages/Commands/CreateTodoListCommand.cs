using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Commands;

namespace Revo.Examples.Todos.Messages.Commands
{
    public class CreateTodoListCommand : ICommand
    {
        public CreateTodoListCommand(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; }

        [Required]
        public string Name { get; }
    }
}
