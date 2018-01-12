using System.Collections.Generic;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Events.Async
{
    public interface IAsyncEventQueueBacklogWorker
    {
        Task RunQueueBacklogAsync(string queueName);
    }
}