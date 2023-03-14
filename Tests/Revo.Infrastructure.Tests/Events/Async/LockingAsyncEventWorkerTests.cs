using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Revo.Domain.Core;
using Revo.Infrastructure.Events.Async;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Async
{
    public class LockingAsyncEventWorkerTests
    {
        [Fact]
        public async Task RunQueueBacklogAsync_LocksAndUnlocks()
        {
            var asyncEventWorker = Substitute.For<IAsyncEventWorker>();
            var asyncEventWorkerLockCache = Substitute.For<IAsyncEventWorkerLockCache>();
            var sut = new LockingAsyncEventWorker(asyncEventWorker, asyncEventWorkerLockCache);

            await sut.RunQueueBacklogAsync("queue1");

            Received.InOrder(() =>
            {
                asyncEventWorkerLockCache.EnsureInitialized();
                asyncEventWorkerLockCache.EnterQueueAsync("queue1");
                asyncEventWorker.RunQueueBacklogAsync("queue1");
                asyncEventWorkerLockCache.ExitQueue("queue1");
            });
        }

        [Fact]
        public async Task RunQueueBacklogAsync_UnlocksOnException()
        {
            var asyncEventWorker = Substitute.For<IAsyncEventWorker>();
            var asyncEventWorkerLockCache = Substitute.For<IAsyncEventWorkerLockCache>();
            var sut = new LockingAsyncEventWorker(asyncEventWorker, asyncEventWorkerLockCache);

            asyncEventWorker.RunQueueBacklogAsync("queue1").Throws(new DomainException());

            await Assert.ThrowsAsync<DomainException>(async () => await sut.RunQueueBacklogAsync("queue1"));

            Received.InOrder(() =>
            {
                asyncEventWorkerLockCache.EnsureInitialized();
                asyncEventWorkerLockCache.EnterQueueAsync("queue1");
                asyncEventWorkerLockCache.ExitQueue("queue1");
            });
        }
    }
}
