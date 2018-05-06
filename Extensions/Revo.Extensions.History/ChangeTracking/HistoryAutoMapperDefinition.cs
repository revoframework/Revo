using AutoMapper;
using Revo.Core.IO;
using Revo.Extensions.History.ChangeTracking.Model;

namespace Revo.Extensions.History.ChangeTracking
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
