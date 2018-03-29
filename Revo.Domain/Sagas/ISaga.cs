using System.Collections.Generic;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Domain.Entities;
using Revo.Domain.Events;

namespace Revo.Domain.Sagas
{
    public interface ISaga : IAggregateRoot
    {
        IEnumerable<ICommandBase> UncommitedCommands { get; }
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> Keys { get; }

        void HandleEvent(IEventMessage<DomainEvent> ev);
    }
}