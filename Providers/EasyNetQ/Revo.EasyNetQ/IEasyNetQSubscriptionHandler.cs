using System.Threading.Tasks;

namespace Revo.EasyNetQ
{
    public interface IEasyNetQSubscriptionHandler
    {
        Task HandleMessageAsync(object message);
    }
}