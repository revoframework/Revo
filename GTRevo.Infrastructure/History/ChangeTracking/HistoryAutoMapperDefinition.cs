using AutoMapper;
using GTRevo.Infrastructure.History.ChangeTracking.Model;
using GTRevo.Platform.Core;

namespace GTRevo.Infrastructure.History.ChangeTracking
{
    public class HistoryAutoMapperDefinition : IAutoMapperDefinition
    {
        private readonly ITrackedChangeRecordConverter trackedChangeRecordConverter;

        public HistoryAutoMapperDefinition(ITrackedChangeRecordConverter trackedChangeRecordConverter)
        {
            this.trackedChangeRecordConverter = trackedChangeRecordConverter;
        }

        public void Configure(IMapperConfigurationExpression config)
        {
            config.CreateMap<TrackedChangeRecord, TrackedChange>()
                .ProjectUsing(x => trackedChangeRecordConverter.FromRecord(x));
        }
    }
}
