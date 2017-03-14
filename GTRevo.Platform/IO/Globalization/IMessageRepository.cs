using GTRevo.Platform.IO.Messages;

namespace GTRevo.Platform.IO.Globalization
{
    public interface IMessageRepository
    {
        IMessageSource GetMessagesForLocale(Locale locale);
        void SetMessagesForLocale(Locale locale, IMessageSource messageSource);
    }
}
