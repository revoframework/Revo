using System.Threading.Tasks;

namespace Revo.Infrastructure.Events.Async
{
    public interface IAsyncEventQueueBacklogWorker
    {
        Task RunQueueBacklogAsync(string queueName);
    }
}