using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Infrastructure.Events.Async;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Async
{
    public class AsyncEventWorkerLockCacheTests
    {
        private readonly AsyncEventWorkerLockCache sut = new AsyncEventWorkerLockCache();

        [Fact]
        public async Task EntersAndExits()
        {
            sut.EnsureInitialized();
            await sut.EnterQueueAsync("queue1");
            sut.ExitQueue("queue1");
        }

        [Fact]
        public async Task EnterIsBlockingWithTwoThreads()
        {
            bool task1Owns = false;
            bool task2Owned = false;
            bool task1ShouldExit = false;
            var task1 = Task.Run(async () =>
            {
                sut.EnsureInitialized();
                await sut.EnterQueueAsync("queue1");
                task1Owns = true;
                while (!task1ShouldExit)
                {
                    await Task.Delay(10);
                }
                sut.ExitQueue("queue1");
            });

            while (!task1Owns)
            {
                Thread.Yield();
            }

            var task2 = Task.Run(async () =>
            {
                sut.EnsureInitialized();
                await sut.EnterQueueAsync("queue1");
                sut.ExitQueue("queue1");
                task2Owned = true;
            });

            Thread.Sleep(100);
            task2Owned.Should().BeFalse();

            task1ShouldExit = true;
            await task1;
            await task2;

            task2Owned.Should().BeTrue();
        }
    }
}
