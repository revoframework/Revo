using System;
using Revo.DataAccess.Entities;
using Revo.Domain.ReadModel;

namespace Revo.Infrastructure.Notifications.Model
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "NBF")]
    public class NotificationBuffer : ReadModelBase
    {
        public NotificationBuffer(Guid id, Guid governorId, Guid pipelineId)
        {
            Id = id;
            GovernorId = governorId;
            PipelineId = pipelineId;
        }

        protected NotificationBuffer()
        {
        }

        public Guid Id { get; private set; }
        public Guid PipelineId { get; private set; }
        public Guid GovernorId { get; set; }
    }
}
