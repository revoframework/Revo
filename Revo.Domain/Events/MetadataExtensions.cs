using System;
using System.Collections.Generic;
using System.Globalization;
using Revo.Core.Events;

namespace Revo.Domain.Events
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

        public static Guid? GetAggregateTenantId(this IReadOnlyDictionary<string, string> metadata)
        {
            string stringValue = GetStringValue(metadata, BasicEventMetadataNames.AggregateTenantId);
            return stringValue?.Length > 0 ? (Guid?) Guid.Parse(stringValue) : null;
        }

        public static int? GetAggregateVersion(this IReadOnlyDictionary<string, string> metadata)
        {
            string stringValue = GetStringValue(metadata, BasicEventMetadataNames.AggregateVersion);
            return stringValue != null ? (int?)int.Parse(GetStringValue(metadata, BasicEventMetadataNames.AggregateVersion)) : null;
        }

        public static long? GetStreamSequenceNumber(this IReadOnlyDictionary<string, string> metadata)
        {
            string stringValue = GetStringValue(metadata, BasicEventMetadataNames.StreamSequenceNumber);
            return stringValue != null ? (long?) long.Parse(GetStringValue(metadata, BasicEventMetadataNames.StreamSequenceNumber)) : null;
        }

        public static DateTimeOffset? GetPublishDate(this IReadOnlyDictionary<string, string> metadata)
        {
            var date = GetStringValue(metadata, BasicEventMetadataNames.PublishDate);
            if (date != null)
            {
                return DateTimeOffset.Parse(date, CultureInfo.InvariantCulture);
            }

            return null;
        }

        public static DateTimeOffset? GetStoreDate(this IReadOnlyDictionary<string, string> metadata)
        {
            var date = GetStringValue(metadata, BasicEventMetadataNames.StoreDate);
            if (date != null)
            {
                return DateTimeOffset.Parse(date, CultureInfo.InvariantCulture);
            }

            return null;
        }

        public static Guid? GetUserId(this IReadOnlyDictionary<string, string> metadata)
        {
            string stringValue = GetStringValue(metadata, BasicEventMetadataNames.UserId);
            return stringValue != null ? (Guid?)Guid.Parse(stringValue) : null;
        }

        public static void ReplaceMetadata(this IEventMessageDraft messageDraft, string key, string newValue)
        {
            if (messageDraft.Metadata.TryGetValue(key, out string oldValue))
            {
                messageDraft.SetMetadata("Original-" + key, oldValue);
            }

            messageDraft.SetMetadata(key, newValue);
        }

        public static void ReplaceMetadata(this IDictionary<string, string> metadata, string key, string newValue)
        {
            if (metadata.TryGetValue(key, out string oldValue))
            {
                metadata["Original-" + key] = oldValue;
            }

            metadata[key] = newValue;
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
