using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Core.Domain.Events
{
    public static class MetadataExtensions
    {
        public static Guid? GetEventId(this IReadOnlyDictionary<string, string> metadata)
        {
            string stringValue = GetStringValue(metadata, BasicEventMetadataNames.EventId);
            return stringValue != null ? (Guid?) Guid.Parse(stringValue) : null;
        }

        public static Guid? GetAggregateClassId(this IReadOnlyDictionary<string, string> metadata)
        {
            string stringValue = GetStringValue(metadata, BasicEventMetadataNames.AggregateClassId);
            return stringValue != null ? (Guid?) Guid.Parse(stringValue) : null;
        }

        public static long? GetStreamSequenceNumber(this IReadOnlyDictionary<string, string> metadata)
        {
            string stringValue = GetStringValue(metadata, BasicEventMetadataNames.StreamSequenceNumber);
            return stringValue != null ? (long?) long.Parse(GetStringValue(metadata, BasicEventMetadataNames.StreamSequenceNumber)) : null;
        }

        public static DateTimeOffset GetPublishDate(this IReadOnlyDictionary<string, string> metadata)
        {
            return DateTimeOffset.Parse(GetStringValue(metadata, BasicEventMetadataNames.PublishDate), CultureInfo.InvariantCulture);
        }

        public static DateTimeOffset GetStoreDate(this IReadOnlyDictionary<string, string> metadata)
        {
            return DateTimeOffset.Parse(GetStringValue(metadata, BasicEventMetadataNames.StoreDate), CultureInfo.InvariantCulture);
        }
        
        private static string GetStringValue(IReadOnlyDictionary<string, string> metadata, string key)
        {
            if (metadata.TryGetValue(key, out string value))
            {
                return value;
            }

            return null;
        }
    }
}
