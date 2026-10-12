using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Revo.Core.Lifecycle;

namespace Revo.EFCore.DataAccess.Entities
{
    /// <summary>
    /// Keeps the DbContexts alive until the end of the current request (if there is one), so that they can outlive
    /// the command that created them - e.g. a query returning an IQueryable that is evaluated only later.
    /// Outside of a request, DbContexts get disposed together with the current scope (e.g. a command's one).
    /// <para>
    /// Thread-safe, as DbContexts can get added concurrently with each other and with the disposal of the cache
    /// (e.g. when <c>AsyncEventProcessor.TryRunQueues</c> processes several synchronous event-listener queues
    /// at once, see <c>SyncQueueProcessingParallelism</c>). A DbContext added after the cache has been disposed
    /// gets disposed right away.
    /// </para>
    /// </summary>
    public class RequestDbContextCache(IRequestLifetime requestLifetime) : IRequestDbContextCache
    {
        private readonly object sync = new object();
        private readonly HashSet<DbContext> dbContexts = new HashSet<DbContext>();
        private bool disposed;

        public void AddDbContext(DbContext dbContext)
        {
            if (requestLifetime.TryRegisterForDispose(dbContext))
            {
                return;
            }

            lock (sync)
            {
                if (disposed)
                {
                    dbContext.Dispose();
                    return;
                }

                dbContexts.Add(dbContext);
            }
        }

        public void Dispose()
        {
            DbContext[] snapshot;
            lock (sync)
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
                snapshot = dbContexts.ToArray();
                dbContexts.Clear();
            }

            foreach (var dbContext in snapshot)
            {
                dbContext.Dispose();
            }
        }
    }
}
