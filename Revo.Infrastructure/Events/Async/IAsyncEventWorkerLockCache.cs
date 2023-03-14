using System.Threading.Tasks;

namespace Revo.Infrastructure.Events.Async
{
    public interface IAsyncEventWorkerLockCache
    {
        void EnsureInitialized();
        Task EnterQueueAsync(string queueName);
        void ExitQueue(string queueName);
    }
}
