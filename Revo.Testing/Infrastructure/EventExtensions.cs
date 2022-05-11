using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Testing.Infrastructure
{
    public static class EventExtensions
    {
        public static IEventMessageDraft<TEvent> ToMessageDraft<TEvent>(this TEvent @event)
            where TEvent : class, IEvent
        {
            Type eventType = @event.GetType();
            Type messageType = typeof(EventMessageDraft<>).MakeGenericType(eventType);
            return (IEventMessageDraft<TEvent>) messageType.GetConstructor(new[] {eventType}).Invoke(new[] {@event});
        }

        public static List<IEventMessageDraft<DomainAggregateEvent>> ToAggregateEventMessages(this IEnumerable<DomainAggregateEvent> events,
            Guid? aggregateClassId = null, long firstSequenceNumber = 0)
        {
            return events.Select((x, i) =>
            {
                var message = x.ToMessageDraft();
                message.SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, (i + firstSequenceNumber + 1).ToString());

                if (aggregateClassId != null)
                {
                    message.SetMetadata(BasicEventMetadataNames.AggregateClassId, aggregateClassId.ToString());
                    message.SetMetadata(BasicEventMetadataNames.StoreDate, Clock.Current.UtcNow.ToString(CultureInfo.InvariantCulture));
                    message.SetMetadata(BasicEventMetadataNames.PublishDate, Clock.Current.UtcNow.ToString(CultureInfo.InvariantCulture));
                }
                return message;
            }).ToList();
        }
    }
}
