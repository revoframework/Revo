using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Core.Domain.Events
{
    public static class BasicEventMetadataNames
    {
        public static readonly string PublishDate = "PublishDate";
        public static readonly string StoreDate = "StoreDate";
        public static readonly string ActorName = "ActorName";
        public static readonly string OriginatorServerHostName = "OriginatorServerHostName";
        public static readonly string UserId = "UserId";
        //public static readonly string StreamVersion = "StreamVersion";
        public static readonly string EventId = "EventId";
        public static readonly string StreamSequenceNumber = "StreamSequenceNumber";
        public static readonly string RequestUri = "RequestUri";
        public static readonly string AggregateClassId = "AggregateClassId";
        public static readonly string MachineName = "MachineName";
        public static readonly string EventSourceName = "EventSourceName";
    }
}
