using System.Threading.Tasks;

namespace Revo.Infrastructure.Events.Async
{
    public interface IAsyncEventWorker
    {
        Task RunQueueBacklogAsync(string queueName);
    }
}