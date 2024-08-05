using System.Text.Json;
using Revo.Core.Events;

namespace Revo.Domain.Events
{
    public abstract class DomainEvent : IEvent
    {
        public override string ToString()
        {
            return $"{GetType().FullName} : {JsonSerializer.Serialize(this, GetType())}";
        }
    }
}
