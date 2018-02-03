using System.Collections.Generic;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;

namespace Revo.Domain.Sagas
{
    public interface ISaga : IEventSourcedAggregateRoot
    {
        IEnumerable<ICommand> UncommitedCommands { get; }
        IReadOnlyDictionary<string, string> Keys { get; }
        bool IsEnded { get; }

        void HandleEvent(IEventMessage<DomainEvent> ev);
    }
}