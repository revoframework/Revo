using System.Threading.Tasks;

namespace Revo.EasyNetQ
{
    public interface IEasyNetQSubscriptionHandler
    {
        Task HandleMessage(object message);
    }
}