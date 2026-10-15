using System;
using System.ComponentModel.DataAnnotations;
using Revo.Core.Commands;

namespace Revo.IntegrationTests.TestApp.Messages.Commands
{
    public class CreateTestListCommand(Guid id, string name) : ICommand
    {
        public Guid Id { get; } = id;

        [Required]
        public string Name { get; } = name;
    }
}
