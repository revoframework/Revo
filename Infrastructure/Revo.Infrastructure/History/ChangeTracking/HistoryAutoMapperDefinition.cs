using AutoMapper;
using Revo.Infrastructure.History.ChangeTracking.Model;
using Revo.Platforms.AspNet.Core;

namespace Revo.Infrastructure.History.ChangeTracking
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
