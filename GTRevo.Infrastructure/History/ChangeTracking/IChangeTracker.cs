using System;
using System.Linq;
using System.Threading.Tasks;
using GTRevo.Platform.Security;

namespace GTRevo.Infrastructure.History.ChangeTracking
{
    public interface IChangeTracker
    {
        TrackedChange AddChange<T>(T changeData, Guid? aggregateId = null, Guid? aggregateClassId = null,
            Guid? entityId = null, Guid? entityClassId = null, Guid? userId = null) where T : ChangeData;

        Task SaveChangesAsync();

        IQueryable<TrackedChange> FindChanges();
        Task<TrackedChange> GetChangeAsync(Guid trackedChangeId);
    }
}
