using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Collections;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Domain.Entities.Basic;
using Revo.Domain.Events;

namespace Revo.Domain.Sagas
{
    /// <summary>
    /// <para>Saga whose state is typically persisted using just its visible state data.</para>
    /// <seealso cref="BasicAggregateRoot"/>
    /// </summary>
    public abstract class BasicSaga : BasicAggregateRoot, IConventionBasedSaga
    {
        private readonly List<ICommandBase> uncommitedCommands = new List<ICommandBase>();
        private readonly MultiValueDictionary<string, string> keys = new MultiValueDictionary<string, string>();
        private readonly IReadOnlyDictionary<Type, IReadOnlyCollection<SagaConventionEventInfo>> events;

        public BasicSaga(Guid id) : base(id)
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
            keys.Remove(name, value);
        }

        protected void RemoveSagaKeys(string name)
        {
            keys.Remove(name);
        }

        protected void AddSagaKey(string name, string value)
        {
            keys.Add(name, value);
        }

        protected void SetSagaKey(string name, string value)
        {
            RemoveSagaKeys(name);
            AddSagaKey(name, value);
        }
    }
}
