using GTRevo.Infrastructure.Globalization.Messages;

namespace GTRevo.Infrastructure.Globalization
{
    public interface ILocaleMessageSourceFactory
    {
        string LocaleCode { get; }
        int Priority { get; }
        IMessageSource MessageSource { get; }
    }
}
