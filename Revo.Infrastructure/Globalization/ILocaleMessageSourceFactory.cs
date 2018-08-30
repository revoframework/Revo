using Revo.Infrastructure.Globalization.Messages;

namespace Revo.Infrastructure.Globalization
{
    public interface ILocaleMessageSourceFactory
    {
        string LocaleCode { get; }
        int Priority { get; }
        IMessageSource MessageSource { get; }
    }
}
