using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Revo.Infrastructure.Events.Async
{
    public class AsyncEventWorkerLockCache : IAsyncEventWorkerLockCache
    {
        private static readonly AsyncLocal<Dictionary<string, IDisposable>> AsyncContextLocks = new AsyncLocal<Dictionary<string, IDisposable>>();

        private readonly ConcurrentDictionary<string, LockRecord> globalLocks = new ConcurrentDictionary<string, LockRecord>();

        public void EnsureInitialized()
        {
            if (AsyncContextLocks.Value == null)
            {
                AsyncContextLocks.Value = new Dictionary<string, IDisposable>();
            }
        }

        public async Task EnterQueueAsync(string queueName)
        {
            if (AsyncContextLocks.Value == null)
            {
                throw new InvalidOperationException("AsyncEventWorkerLockCache has not been initialized on the active async context");
            }

            if (AsyncContextLocks.Value.ContainsKey(queueName))
            {
                throw new InvalidOperationException($"Async event queue '{queueName}' is already locked in the current async context");
            }

            LockRecord lockRecord = globalLocks.AddOrUpdate(queueName,
                name => new LockRecord(new AsyncLock(), 1),
                (name, existing) => new LockRecord(existing.Lock, existing.ReferenceCount + 1));

            var disposable = await lockRecord.Lock.LockAsync();
            AsyncContextLocks.Value[queueName] = disposable;
        }

        public void ExitQueue(string queueName)
        {
            if (AsyncContextLocks.Value == null)
            {
                throw new InvalidOperationException("AsyncEventWorkerLockCache has not been initialized on the active async context");
            }

            if (!globalLocks.TryGetValue(queueName, out LockRecord lockRecord))
            {
                throw new InvalidOperationException(
                    $"There is no lock object for async event queue worker: {queueName}");
            }

            if (!AsyncContextLocks.Value.TryGetValue(queueName, out IDisposable lockDisposable))
            {
                throw new InvalidOperationException(
                    $"There is no lock for Async event queue '{queueName}' in the current async context");
            }

            if (lockRecord.ReferenceCount == 1)
            {
                var locksCollection = (ICollection<KeyValuePair<string, LockRecord>>)globalLocks;
                locksCollection.Remove(new KeyValuePair<string, LockRecord>(queueName, lockRecord));
            }

            AsyncContextLocks.Value.Remove(queueName);
            lockDisposable.Dispose();
        }

        public class LockRecord
        {
            public LockRecord(AsyncLock @lock, int referenceCount)
            {
                Lock = @lock;
                ReferenceCount = referenceCount;
            }

            public AsyncLock Lock { get; }
            public int ReferenceCount { get; }
        }
    }
}
