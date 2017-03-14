using Newtonsoft.Json.Linq;

namespace GTRevo.Infrastructure.EventSourcing.EF6.Model
{
    public class DomainEventSerialized
    {
        public string EventName { get; set; }
        public int EventVersion { get; set; }
        public JObject Data { get; set; }
    }
}
