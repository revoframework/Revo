using System.Collections.Generic;
using System.Linq;
using GTRevo.Infrastructure.Globalization.Messages;

namespace GTRevo.Infrastructure.Globalization
{
    public class MessageRepository : IMessageRepository
    {
        private readonly Dictionary<string, IMessageSource> localeSources = new Dictionary<string, IMessageSource>();
        private readonly ILocaleMessageSourceFactory[] localeMessageSourceFactories;

        public MessageRepository(ILocaleMessageSourceFactory[] localeMessageSourceFactories)
        {
            this.localeMessageSourceFactories = localeMessageSourceFactories;
        }

        public IMessageSource GetMessagesForLocale(Locale locale)
        {
            IMessageSource messageSource = null;
            if (!localeSources.TryGetValue(locale.Code, out messageSource))
            {
                messageSource = new CompositeMessageSource(
                    localeMessageSourceFactories
                        .Where(x => x.LocaleCode == locale.Code)
                        .Select(x => x.MessageSource));
                localeSources[locale.Code] = messageSource;
            }

            return messageSource;
        }
    }
}
