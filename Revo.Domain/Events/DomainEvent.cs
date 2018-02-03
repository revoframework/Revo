using Newtonsoft.Json;
using Revo.Core.Events;

namespace Revo.Domain.Events
{
    public abstract class DomainEvent : IEvent
    {
        public override string ToString()
        {
            return $"{this.GetType().FullName} : {JsonConvert.SerializeObject(this)}";
        }
    }
}
