using System;
using Revo.Core.Commands;

namespace Revo.IntegrationTests.TestApp.Messages.Commands
{
    /// <summary>
    /// Creates a list and sends nested commands creating the child lists from within its handler.
    /// </summary>
    public class CreateTestListWithChildrenCommand(Guid id, string name, Guid[] childIds) : ICommand
    {
        public Guid Id { get; } = id;
        public string Name { get; } = name;
        public Guid[] ChildIds { get; } = childIds;
    }
}
