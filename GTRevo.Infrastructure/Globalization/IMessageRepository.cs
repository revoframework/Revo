using System.Threading.Tasks;
using GTRevo.Infrastructure.Globalization.Messages;

namespace GTRevo.Infrastructure.Globalization
{
    public interface IMessageRepository
    {
        IMessageSource GetMessagesForLocale(Locale locale);
        Task ReloadAsync();
    }
}
