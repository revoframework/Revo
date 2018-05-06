using Revo.Extensions.History.ChangeTracking.Model;

namespace Revo.Extensions.History.ChangeTracking
{
    public interface ITrackedChangeRecordConverter
    {
        TrackedChange FromRecord(TrackedChangeRecord record);
        TrackedChangeRecord ToRecord(TrackedChange change);
    }
}