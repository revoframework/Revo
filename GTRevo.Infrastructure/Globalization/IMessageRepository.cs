using GTRevo.Infrastructure.Globalization.Messages;

namespace GTRevo.Infrastructure.Globalization
{
    public interface IMessageRepository
    {
        IMessageSource GetMessagesForLocale(Locale locale);
        //void SetMessagesForLocale(Locale locale, IMessageSource messageSource);
    }
}
