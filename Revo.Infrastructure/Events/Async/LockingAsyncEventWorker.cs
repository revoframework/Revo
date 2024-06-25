using System.Threading.Tasks;

namespace Revo.Infrastructure.Events.Async
{
    /// <summary>
    /// Async event worker decorator allowing only one processing of a queue at a time.
    /// Limits the amount of optimistic concurrency exceptions if there is a possibility of multiple parallel
    /// workers being spawned (or prevents them, if they are only run from one process).
    /// </summary>
    public class LockingAsyncEventWorker(IAsyncEventWorker asyncEventWorkerImplementation,
            IAsyncEventWorkerLockCache asyncEventWorkerLockCache) : IAsyncEventWorker
    {
        public async Task RunQueueBacklogAsync(string queueName)
        {
            asyncEventWorkerLockCache.EnsureInitialized();
            await asyncEventWorkerLockCache.EnterQueueAsync(queueName);

            try
            {
                await asyncEventWorkerImplementation.RunQueueBacklogAsync(queueName);
            }
            finally
            {
                asyncEventWorkerLockCache.ExitQueue(queueName);
            }
        }
    }
}
