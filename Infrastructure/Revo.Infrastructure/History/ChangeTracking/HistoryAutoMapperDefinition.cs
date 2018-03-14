using AutoMapper;
using Revo.Core.IO;
using Revo.Infrastructure.History.ChangeTracking.Model;

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
