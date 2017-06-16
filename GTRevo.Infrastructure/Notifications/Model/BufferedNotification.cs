using System;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.ReadModel;

namespace GTRevo.Infrastructure.Notifications.Model
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "BNT")]
    public class BufferedNotification : ReadModelBase
    {
        public BufferedNotification(Guid id, string notificationClassName,
            string notificationJson, NotificationBuffer buffer, DateTime timeQueued)
        {
            Id = id;
            NotificationClassName = notificationClassName;
            NotificationJson = notificationJson;
            Buffer = buffer;
            TimeQueued = timeQueued;
        }

        protected BufferedNotification()
        { 
        }

        public Guid Id { get; private set; }
        public string NotificationClassName { get; private set; }
        public string NotificationJson { get; private set; }
        public NotificationBuffer Buffer { get; private set; }
        public DateTime TimeQueued { get; private set; }
    }
}
