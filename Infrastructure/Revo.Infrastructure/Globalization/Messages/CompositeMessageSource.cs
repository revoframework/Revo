using System.Collections.Generic;
using System.Collections.Immutable;

namespace Revo.Infrastructure.Globalization.Messages
{
    public class CompositeMessageSource : IMessageSource
    {
        public CompositeMessageSource(IEnumerable<IMessageSource> messageSources)
        {
            var messagesBuilder = ImmutableDictionary.CreateBuilder<string, string>();
            foreach (var messageSource in messageSources)
            {
                foreach (var msg in messageSource.Messages)
                {
                    messagesBuilder[msg.Key] = msg.Value;
                }
            }
            
            Messages = messagesBuilder.ToImmutable();
        }

        public ImmutableDictionary<string, string> Messages { get; }
        public bool TryGetMessage(string key, out string message) => Messages.TryGetValue(key, out message);

    }
}
