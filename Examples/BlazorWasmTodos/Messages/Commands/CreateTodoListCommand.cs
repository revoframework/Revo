﻿using System.ComponentModel.DataAnnotations;
using Revo.Core.Commands;

namespace Revo.Examples.BlazorWasmTodos.Messages.Commands
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
