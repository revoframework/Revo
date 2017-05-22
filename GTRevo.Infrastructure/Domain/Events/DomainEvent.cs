using System;
using GTRevo.Platform.Events;
using Newtonsoft.Json;

namespace GTRevo.Infrastructure.Domain.Events
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
