using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Entities
{
    /// <summary>
    /// Request-scoped set of <see cref="DbContext"/> instances created during the request.
    /// Task-scoped database access does not dispose them; this cache does, when the request ends.
    /// <para>
    /// After a command commits, <c>AsyncEventProcessor.TryRunQueues</c> can run several
    /// synchronous event-listener queues at once (<c>SyncQueueProcessingParallelism</c>).
    /// Each queue has its own task scope and therefore its own <see cref="DbContext"/>, but
    /// those tasks inherit the HTTP request scope, so they all add to this same set.
    /// The first use of a context calls <see cref="AddDbContext"/> from those threads together,
    /// and request disposal can overlap that. The set is locked so concurrent add and dispose
    /// do not corrupt it.
    /// </para>
    /// </summary>
    public class RequestDbContextCache : IRequestDbContextCache
    {
        private readonly object sync = new object();
        private readonly HashSet<DbContext> dbContexts = new HashSet<DbContext>();
        private bool disposed;

        public void AddDbContext(DbContext dbContext)
        {
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
