using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;

namespace Revo.Domain.Sagas
{
    public class Saga : EventSourcedAggregateRoot, ISaga
    {
        private readonly List<ICommand> uncommitedCommands = new List<ICommand>();
        private readonly MultiValueDictionary<string, string> keys = new MultiValueDictionary<string, string>();
        private readonly IReadOnlyDictionary<Type, SagaConventionEventInfo> events;

        public Saga(Guid id) : base(id)
        {
            events = SagaConventionConfigurationCache.GetSagaConfigurationInfo(this.GetType()).Events;
        }

        public override bool IsChanged => base.IsChanged || uncommitedCommands.Any();

        public IEnumerable<ICommand> UncommitedCommands => uncommitedCommands;
        public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Keys => keys;

        public bool IsEnded { get; }

        public void HandleEvent(IEventMessage<DomainEvent> ev)
        {
            if (events.TryGetValue(ev.Event.GetType(), out SagaConventionEventInfo eventInfo))
            {
                eventInfo.HandleDelegate(this, ev);
            }
        }

        public override void Commit()
        {
            base.Commit();
            uncommitedCommands.Clear();
        }

        protected void SendCommand(ICommand command)
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
