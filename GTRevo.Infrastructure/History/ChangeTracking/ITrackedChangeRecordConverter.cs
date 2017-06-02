using GTRevo.Infrastructure.History.ChangeTracking.Model;

namespace GTRevo.Infrastructure.History.ChangeTracking
{
    public interface ITrackedChangeRecordConverter
    {
        TrackedChange FromRecord(TrackedChangeRecord record);
        TrackedChangeRecord ToRecord(TrackedChange change);
    }
}