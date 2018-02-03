using Revo.Infrastructure.History.ChangeTracking.Model;

namespace Revo.Infrastructure.History.ChangeTracking
{
    public interface ITrackedChangeRecordConverter
    {
        TrackedChange FromRecord(TrackedChangeRecord record);
        TrackedChangeRecord ToRecord(TrackedChange change);
    }
}