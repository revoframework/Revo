using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Events;
using Nito.AsyncEx;

namespace GTRevo.Infrastructure.Globalization.Messages.Database
{
    public class DbMessageCache : IDbMessageCache
    {
        private Dictionary<string, ImmutableDictionary<string, string>> messages;
        private readonly IEventBus eventBus;

        public DbMessageCache(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public bool IsInitialized => messages != null;

        public ImmutableDictionary<string, string> GetLocaleMessages(string localeCode)
        {
            if (messages != null && messages.TryGetValue(localeCode, out var localeMessages))
            {
                return localeMessages;
            }

            return ImmutableDictionary<string, string>.Empty;
        }

        public void ReplaceMessages(IEnumerable<LocalizationMessage> data)
        {
            var newMessages = new Dictionary<string, ImmutableDictionary<string, string>.Builder>();
            foreach (var item in data)
            {
                if (!newMessages.TryGetValue(item.LocaleCode, out var localeMessages))
                {
                    newMessages[item.LocaleCode] = localeMessages = ImmutableDictionary.CreateBuilder<string, string>();
                }

                localeMessages[item.ClassName?.Length > 0 ? $"{item.ClassName}.{item.Key}" : item.Key] = item.Message;
            }

            messages = newMessages.ToDictionary(x => x.Key,
                x => x.Value.ToImmutable());

            eventBus.PublishAsync(
                new EventMessageDraft<DbMessageCacheReloadedEvent>(new DbMessageCacheReloadedEvent()));

            // TODO deadlocks even though it shouldn't (?)
            /*Task.Run(() =>
                eventBus.PublishAsync(
                    new EventMessageDraft<DbMessageCacheReloadedEvent>(new DbMessageCacheReloadedEvent())))
                    .GetAwaiter().GetResult();*/
        }
    }
}
