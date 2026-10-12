using System;
using Revo.Core.Commands;

namespace Revo.IntegrationTests.TestApp.Messages.Commands
{
    /// <summary>
    /// Creates a list, sends a nested command creating another one and then fails.
    /// </summary>
    public class CreateTestListThenFailCommand(Guid id, Guid nestedId) : ICommand
    {
        public Guid Id { get; } = id;
        public Guid NestedId { get; } = nestedId;
    }
}
