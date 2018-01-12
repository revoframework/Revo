using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Events;
using GTRevo.Infrastructure.Globalization.Messages;
using Nito.AsyncEx;

namespace GTRevo.Infrastructure.Globalization
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ILocaleMessageSourceFactory[] localeMessageSourceFactories;
        private Dictionary<string, IMessageSource> localeSources;
        private readonly IEventBus eventBus;

        public MessageRepository(ILocaleMessageSourceFactory[] localeMessageSourceFactories,
            IEventBus eventBus)
        {
            this.localeMessageSourceFactories = localeMessageSourceFactories;
            this.eventBus = eventBus;

            DoReload();
        }

        public IMessageSource GetMessagesForLocale(Locale locale)
        {
            if (localeSources.TryGetValue(locale.Code, out IMessageSource messageSource))
            {
                return messageSource;
            }

            return null;
        }

        public async Task ReloadAsync()
        {
            DoReload();
            await eventBus.PublishAsync(
                new EventMessageDraft<MessageRepositoryReloadedEvent>(new MessageRepositoryReloadedEvent()));
        }

        private void DoReload()
        {
            var newLocaleSources = new Dictionary<string, IMessageSource>();

            foreach (var factoriesByLocale in localeMessageSourceFactories.GroupBy(x => x.LocaleCode))
            {
                var messageSource = new CompositeMessageSource(
                    factoriesByLocale
                        .OrderBy(x => x.Priority)
                        .Select(x => x.MessageSource));
                newLocaleSources[factoriesByLocale.Key] = messageSource;
            }

            localeSources = newLocaleSources;
        }
    }
}
