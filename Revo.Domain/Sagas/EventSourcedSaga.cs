using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Revo.Core.Collections;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Domain.Sagas.Events;

namespace Revo.Domain.Sagas
{
    /// <summary>
    /// <para>Saga that uses event sourcing to define its state.</para>
    /// <seealso cref="EventSourcedAggregateRoot"/>
    /// </summary>
    public abstract class EventSourcedSaga : EventSourcedAggregateRoot, IConventionBasedSaga
    {
        private readonly List<ICommandBase> uncommitedCommands = new List<ICommandBase>();
        private readonly MultiValueDictionary<string, string> keys = new MultiValueDictionary<string, string>();
        private readonly IReadOnlyDictionary<Type, IReadOnlyCollection<SagaConventionEventInfo>> events;

        public EventSourcedSaga(Guid id) : base(id)
        {
            events = SagaConventionConfigurationCache.GetSagaConfigurationInfo(this.GetType()).Events;
        }

        public override bool IsChanged => base.IsChanged || uncommitedCommands.Any();

        public IEnumerable<ICommandBase> UncommitedCommands => uncommitedCommands;
        public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Keys => keys;
        
        public void HandleEvent(IEventMessage<DomainEvent> ev)
        {
            if (events.TryGetValue(ev.Event.GetType(), out var eventInfos))
            {
                var first = eventInfos.First(); // because we use Apply(T) convention, all handle delegates should be the same
                first.HandleDelegate(this, ev);
            }
        }

        public override void Commit()
        {
            base.Commit();
            uncommitedCommands.Clear();
        }

        protected void AddSagaKey(string name, string value)
        {
            var newKeys = new MultiValueDictionary<string, string>(keys);
            newKeys.Add(name, value);

            Publish(new SagaKeysChangedEvent(
                newKeys
                    .Select(x => new KeyValuePair<string, ImmutableList<string>>(x.Key, x.Value.ToImmutableList()))
                    .ToImmutableDictionary()));
        }

        protected void Send(ICommandBase command)
        {
            uncommitedCommands.Add(command);
        }

        protected string GetSagaKeyOrDefault(string name)
        {
            if (keys.TryGetValue(name, out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }

        protected string[] GetSagaKeys(string name)
        {
            if (keys.TryGetValue(name, out var values))
            {
                return values.ToArray();
            }

            return null;
        }

        protected void RemoveSagaKey(string name, string value)
        {
            bool changed = false;
            var newKeys =
                keys.Select(x =>
                {
                    if (x.Key == name)
                    {
                        var newValues = x.Value.ToList();
                        changed = newValues.Remove(value);
                        return new KeyValuePair<string, ImmutableList<string>>(x.Key, newValues.ToImmutableList());
                    }

                    return new KeyValuePair<string, ImmutableList<string>>(x.Key, x.Value.ToImmutableList());
                }).ToImmutableDictionary();

            if (changed)
            {
                Publish(new SagaKeysChangedEvent(newKeys));
            }
        }

        protected void RemoveSagaKeys(string name)
        {
            if (keys.ContainsKey(name))
            {
                Publish(new SagaKeysChangedEvent(
                   keys
                    .Where(x => x.Key != name)
                    .Select(x => new KeyValuePair<string, ImmutableList<string>>(x.Key, x.Value.ToImmutableList()))
                    .ToImmutableDictionary()));
            }
        }

        protected void SetSagaKey(string name, string value)
        {
            var newKeys = new MultiValueDictionary<string, string>(keys);
            newKeys.Remove(name);
            newKeys.Add(name, value);

            Publish(new SagaKeysChangedEvent(
                newKeys
                    .Select(x => new KeyValuePair<string, ImmutableList<string>>(x.Key, x.Value.ToImmutableList()))
                    .ToImmutableDictionary()));
        }

        private void Apply(SagaKeysChangedEvent ev)
        {
            keys.Clear();
            foreach (var namePair in ev.Keys)
            {
                keys.AddRange(namePair.Key, namePair.Value);
            }
        }
    }
}
