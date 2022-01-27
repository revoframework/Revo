using System;
using System.Collections.Generic;
using Revo.DataAccess.Entities;
using Revo.Domain.ReadModel;

namespace Revo.Extensions.Notifications.Model
{
    [TablePrefix(NamespacePrefix = "RNO", ColumnPrefix = "NBF")]
    public class NotificationBuffer : ReadModelBase
    {
        public NotificationBuffer(Guid id, string name, string governorName, string pipelineName)
        {
            Id = id;
            Name = name;
            GovernorName = governorName;
            PipelineName = pipelineName;
        }

        protected NotificationBuffer()
        {
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string PipelineName { get; private set; }
        public string GovernorName { get; set; }
        public List<BufferedNotification> Notifications { get; set; }
    }
}
