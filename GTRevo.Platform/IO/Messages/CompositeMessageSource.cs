using System.Collections.Generic;
using System.Linq;

namespace GTRevo.Platform.IO.Messages
{
    public class CompositeMessageSource : IMessageSource
    {
        private readonly List<IMessageSource> messageSources;
        private readonly Dictionary<string, string> messages = new Dictionary<string, string>();

        public CompositeMessageSource(IEnumerable<IMessageSource> messageSources)
        {
            this.messageSources = messageSources.ToList();

            for (int i = 0; i < this.messageSources.Count; i++)
            {
                foreach (var msg in this.messageSources[i].Messages)
                {
                    if (!messages.ContainsKey(msg.Key))
                    {
                        messages[msg.Key] = msg.Value;
                    }
                }
            }
        }

        public IEnumerable<KeyValuePair<string, string>> Messages => messages;

        public bool TryGetMessage(string key, out string message)
        {
            for (int i = 0; i < messageSources.Count; i++)
            {
                if (messageSources[i].TryGetMessage(key, out message))
                    return true;
            }

            message = null;
            return false;
        }
    }
}
