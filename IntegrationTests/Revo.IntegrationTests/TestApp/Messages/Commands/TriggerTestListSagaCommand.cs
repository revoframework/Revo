using System;
using Revo.Core.Commands;

namespace Revo.IntegrationTests.TestApp.Messages.Commands
{
    public class TriggerTestListSagaCommand(Guid id) : ICommand
    {
        public Guid Id { get; } = id;
    }
}
