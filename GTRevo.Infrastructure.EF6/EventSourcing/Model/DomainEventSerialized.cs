using Newtonsoft.Json.Linq;

namespace GTRevo.Infrastructure.EF6.EventSourcing.Model
{
    public class DomainEventSerialized
    {
        public string EventName { get; set; }
        public int EventVersion { get; set; }
        public JObject Data { get; set; }
    }
}
