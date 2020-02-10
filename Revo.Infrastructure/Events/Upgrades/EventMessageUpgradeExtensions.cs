using System;
using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Events.Upgrades
{
    public static class EventMessageUpgradeExtensions
    {
        public static IEnumerable<IEventMessage<DomainAggregateEvent>> Replace<TSource>(
            this IEnumerable<IEventMessage<DomainAggregateEvent>> eventStream,
            Func<IEventMessage<TSource>, IEnumerable<IEventMessage<DomainAggregateEvent>>> upgradeFunction)
        where TSource : DomainAggregateEvent
        {
            foreach (var message in eventStream)
            {
                if (message.Event is TSource)
                {
                    var newMessages = upgradeFunction(message as IEventMessage<TSource>);
                    foreach (var newMessage in newMessages)
                    {
                        yield return newMessage;
                    }
                }

                yield return message;
            }
        }
        
        public static IEnumerable<IEventMessage<DomainAggregateEvent>> Replace<TSource>(
            this IEnumerable<IEventMessage<DomainAggregateEvent>> eventStream,
            Func<IEventMessage<TSource>, IEnumerable<DomainAggregateEvent>> upgradeFunction)
        where TSource : DomainAggregateEvent
        {
            foreach (var message in eventStream)
            {
                if (message.Event is TSource)
                {
                    var newMessages = upgradeFunction(message as IEventMessage<TSource>);
                    foreach (var newMessage in newMessages)
                    {
                        yield return (IEventMessage<DomainAggregateEvent>)UpgradedEventMessage.Create(message, newMessage);
                    }
                }

                yield return message;
            }
        }

        public static IEnumerable<IEventMessage<DomainAggregateEvent>> Remove<TSource>(
            this IEnumerable<IEventMessage<DomainAggregateEvent>> eventStream)
        where TSource : DomainAggregateEvent
        {
            foreach (var message in eventStream)
            {
                if (!(message.Event is TSource))
                {
                    yield return message;
                }
            }
        }
    }
}