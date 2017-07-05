using System;
using GTRevo.Core.Events;
using Newtonsoft.Json;

namespace GTRevo.Infrastructure.Core.Domain.Events
{
    public abstract class DomainEvent : IEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public override string ToString()
        {
            return $"{this.GetType().FullName} : {JsonConvert.SerializeObject(this)}";
        }
    }
}
