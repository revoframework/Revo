using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Infrastructure.Globalization.Messages;

namespace Revo.Infrastructure.Globalization
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
