using System.Threading.Tasks;
using Revo.Infrastructure.Globalization.Messages;

namespace Revo.Infrastructure.Globalization
{
    public interface IMessageRepository
    {
        IMessageSource GetMessagesForLocale(Locale locale);
        Task ReloadAsync();
    }
}
