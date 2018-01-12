using System.Collections.Generic;
using GTRevo.Core.Commands;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;

namespace GTRevo.Infrastructure.Core.Domain
{
    public interface ISaga : IEventSourcedAggregateRoot
    {
        IEnumerable<ICommand> UncommitedCommands { get; }
        IReadOnlyDictionary<string, string> Keys { get; }
        bool IsEnded { get; }

        void HandleEvent(IEventMessage<DomainEvent> ev);
    }
}