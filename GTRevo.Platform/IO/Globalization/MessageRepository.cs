using System.Collections.Generic;
using GTRevo.Platform.IO.Messages;

namespace GTRevo.Platform.IO.Globalization
{
    public class MessageRepository : IMessageRepository
    {
        private Dictionary<Locale, IMessageSource> localeMessages = new Dictionary<Locale, IMessageSource>();

        public IMessageSource GetMessagesForLocale(Locale locale)
        {
            IMessageSource messageSource = null;
            localeMessages.TryGetValue(locale, out messageSource);
            return messageSource;
        }

        public void SetMessagesForLocale(Locale locale, IMessageSource messageSource)
        {
            localeMessages[locale] = messageSource;
        }
    }
}
