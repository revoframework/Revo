using System.Collections.Immutable;
using Revo.Domain.Events;

namespace Revo.Domain.Sagas.Events
{
    public class SagaKeysChangedEvent : DomainAggregateEvent
    {
        public SagaKeysChangedEvent(ImmutableDictionary<string, ImmutableList<string>> keys)
        {
            Keys = keys;
        }

        public ImmutableDictionary<string, ImmutableList<string>> Keys { get; private set; }
    }
}
