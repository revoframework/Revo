using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public string Name { get; }
    }
}
