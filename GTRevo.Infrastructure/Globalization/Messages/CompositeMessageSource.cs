using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GTRevo.Infrastructure.Globalization.Messages
{
    public class CompositeMessageSource : IMessageSource
    {
        private readonly List<IMessageSource> messageSources;
        private readonly ImmutableDictionary<string, string> messages;

        public CompositeMessageSource(IEnumerable<IMessageSource> messageSources)
        {
            this.messageSources = messageSources.ToList();

            var messagesBuilder = ImmutableDictionary.CreateBuilder<string, string>();
            for (int i = 0; i < this.messageSources.Count; i++)
            {
                foreach (var msg in this.messageSources[i].Messages)
                {
                    if (!messagesBuilder.ContainsKey(msg.Key))
                    {
                        messagesBuilder[msg.Key] = msg.Value;
                    }
                }
            }

            messages = messagesBuilder.ToImmutable();
        }

        public ImmutableDictionary<string, string> Messages => messages;

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
