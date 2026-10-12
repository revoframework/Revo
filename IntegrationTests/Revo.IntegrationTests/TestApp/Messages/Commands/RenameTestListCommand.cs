using System;
using Revo.Core.Commands;

namespace Revo.IntegrationTests.TestApp.Messages.Commands
{
    public class RenameTestListCommand(Guid id, string name) : ICommand
    {
        public Guid Id { get; } = id;
        public string Name { get; } = name;
    }
}
