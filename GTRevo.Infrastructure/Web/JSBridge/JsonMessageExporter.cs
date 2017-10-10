using GTRevo.Infrastructure.Globalization.Messages;
using Newtonsoft.Json.Linq;

namespace GTRevo.Infrastructure.Web.JSBridge
{
    public class JsonMessageExporter
    {
        public JObject ExportMessages(IMessageSource messageSource)
        {
            JObject dictionary = new JObject();
            foreach (var msg in messageSource.Messages)
            {
                dictionary[msg.Key] = msg.Value;
            }

            return dictionary;
        }
    }
}
