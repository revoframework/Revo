using System;
using System.Collections.Generic;

namespace Revo.Core.Events
{
    public static class EventMessageDraftExtensions
    {
        public static IEventMessageDraft Clone(this IEventMessage message)
        {
            Type messageType = typeof(EventMessageDraft<>).MakeGenericType(message.Event.GetType());
            return (IEventMessageDraft)messageType.GetConstructor(
                    new[] { message.Event.GetType(), typeof(IReadOnlyDictionary<string, string>) })
                .Invoke(new object[] { message.Event, message.Metadata });
        }

        public static IEventMessageDraft SetId(this IEventMessageDraft message, Guid id)
        {
            return message.SetMetadata(BasicEventMetadataNames.EventId, id.ToString());
        }
    }
}