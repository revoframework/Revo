using AutoMapper;
using Revo.Extensions.History.ChangeTracking.Model;

namespace Revo.Extensions.History.ChangeTracking
{
    public class HistoryAutoMapperProfile : Profile
    {
        public HistoryAutoMapperProfile(ITrackedChangeRecordConverter trackedChangeRecordConverter)
        {
            CreateMap<TrackedChangeRecord, TrackedChange>()
                .ConvertUsing(x => trackedChangeRecordConverter.FromRecord(x));
        }
    }
}
