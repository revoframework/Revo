using System.Collections.Immutable;
using Revo.Domain.Events;

namespace Revo.Domain.Sagas.Events
{
    public class SagaKeysChangedEvent(ImmutableDictionary<string, ImmutableList<string>> keys)
        : DomainAggregateEvent
    {
        public ImmutableDictionary<string, ImmutableList<string>> Keys { get; private set; } = keys;
    }
}
