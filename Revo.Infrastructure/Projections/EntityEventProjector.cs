using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Revo.Core.Collections;
using Revo.Core.Events;
using Revo.Domain.Events;
using Revo.Infrastructure.Events;

namespace Revo.Infrastructure.Projections
{
    /// <summary>
    /// An event projector for an aggregate type with arbitrary read-model(s).
    /// A convention-based abstract base class that calls an Apply for every event type
    /// and also supports sub-projectors.
    /// </summary>
    public abstract class EntityEventProjector : IEventPublishingEntityEventProjector
    {
        private readonly Lazy<MultiValueDictionary<Type, Func<IEventMessage<DomainAggregateEvent>, Task>>> applyHandlers;
        private readonly List<ISubEntityEventProjector> subProjectors = new List<ISubEntityEventProjector>();
        private readonly List<IEvent> publishedEvents = new List<IEvent>();

        public EntityEventProjector()
        {
            applyHandlers = new Lazy<MultiValueDictionary<Type, Func<IEventMessage<DomainAggregateEvent>, Task>>>(CreateApplyDelegates);
        }
        
        public IPublishEventBuffer EventBuffer { get; set; }
        public IEventMessageFactory EventMessageFactory { get; set; }

        protected Guid AggregateId { get; private set; }
        protected IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> Events { get; private set; }

        public virtual async Task CommitChangesAsync()
        {
            await PublishEventsAsync();
        }

        public virtual async Task ProjectEventsAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events)
        {
            if (events.Count == 0)
            {
                throw new InvalidOperationException($"No events to project for aggregate with ID {aggregateId}");
            }
            
            AggregateId = aggregateId;
            Events = events;

            try
            {
                await ApplyEventsAsync(events);
            }
            finally
            {
                AggregateId = default(Guid);
                Events = null;
            }
        }

        protected void AddSubProjector(ISubEntityEventProjector projector)
        {
            subProjectors.Add(projector);

            if (applyHandlers.IsValueCreated)
            {
                CreateApplyDelegates(projector.GetType(), projector, applyHandlers.Value);
            }
        }

        protected void PublishEvent(IEvent ev)
        {
            publishedEvents.Add(ev);
        }

        protected async Task ExecuteHandlerAsync<T>(T evt) where T : IEventMessage<DomainAggregateEvent>
        {
            IReadOnlyCollection<Func<IEventMessage<DomainAggregateEvent>, Task>> handlers;
            if (applyHandlers.Value.TryGetValue(evt.Event.GetType(), out handlers))
            {
                foreach (var handler in handlers)
                {
                    await handler(evt);
                }
            }
        }

        protected virtual async Task ApplyEventsAsync(IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events)
        {
            foreach (IEventMessage<DomainAggregateEvent> ev in events)
            {
                await ExecuteHandlerAsync(ev);
            }
        }

        protected virtual IEnumerable<(Type EventType, Func<IEventMessage<DomainAggregateEvent>, Task> Delegate)> GetApplyDelegates(
            Type projectorType, object instance)
        {
            var actions = projectorType
                .GetMethods(BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public |
                            BindingFlags.NonPublic)
                .Where(x => x.Name == "Apply"
                            && x.DeclaringType == projectorType
                            && x.GetBaseDefinition() == x //exclude overrides
                            && x.GetParameters().Length == 1
                            && typeof(IEventMessage<DomainAggregateEvent>).IsAssignableFrom(x.GetParameters()[0]
                                .ParameterType))
                .Select(x =>
                    (
                        EventType: x.GetParameters()[0].ParameterType.GetGenericArguments()[0],
                        Delegate: (Func<IEventMessage<DomainAggregateEvent>, Task>) (ev =>
                        {
                            Task ret = x.Invoke(instance, new object[] { ev }) as Task;
                            if (ret != null)
                            {
                                return ret;
                            }

                            return Task.FromResult(0);
                        })
                    ));

            actions = actions.Concat(
                projectorType
                    .GetMethods(BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public |
                                BindingFlags.NonPublic)
                    .Where(x => x.Name == "Apply"
                                && x.DeclaringType == projectorType
                                && x.GetBaseDefinition() == x //exclude overrides
                                && x.GetParameters().Length == 2
                                && typeof(IEventMessage<DomainAggregateEvent>).IsAssignableFrom(x.GetParameters()[0]
                                    .ParameterType)
                                && x.GetParameters()[1].ParameterType.IsAssignableFrom(typeof(Guid)))
                    .Select(x =>
                    (
                        EventType: x.GetParameters()[0].ParameterType.GetGenericArguments()[0],
                        Delegate: (Func<IEventMessage<DomainAggregateEvent>, Task>)(ev =>
                        {
                            Task ret = x.Invoke(instance, new object[] { ev, this.AggregateId }) as Task;
                            if (ret != null)
                            {
                                return ret;
                            }

                            return Task.FromResult(0);
                        })
                    ))
                );

            return actions;
        }

        private MultiValueDictionary<Type, Func<IEventMessage<DomainAggregateEvent>, Task>> CreateApplyDelegates()
        {
            var result = new MultiValueDictionary<Type, Func<IEventMessage<DomainAggregateEvent>, Task>>();
            CreateApplyDelegates(GetType(), this, result);

            foreach (var subProjector in subProjectors)
            {
                CreateApplyDelegates(subProjector.GetType(), subProjector, result);
            }

            return result;
        }

        private void CreateApplyDelegates(Type projectorType, object instance, MultiValueDictionary<Type, Func<IEventMessage<DomainAggregateEvent>, Task>> result)
        {
            if (projectorType.BaseType != null)
            {
                CreateApplyDelegates(projectorType.BaseType, instance, result);
            }

            var actions = GetApplyDelegates(projectorType, instance);

            foreach (var action in actions)
            {
                result.Add(action.Item1, action.Item2);
            }
        }

        private async Task PublishEventsAsync()
        {
            if (publishedEvents.Count > 0)
            {
                if (EventBuffer == null)
                {
                    throw new InvalidOperationException($"Cannot publish events from {this} because its {nameof(EventBuffer)} has not been set");
                }

                if (EventMessageFactory == null)
                {
                    throw new InvalidOperationException($"Cannot publish events from {this} because its {nameof(EventMessageFactory)} has not been set");
                }
            }

            foreach (var ev in publishedEvents)
            {
                var eventMessage = await EventMessageFactory.CreateMessageAsync(ev);
                if (eventMessage.Metadata.GetEventId() == null)
                {
                    eventMessage.SetMetadata(BasicEventMetadataNames.EventId, Guid.NewGuid().ToString());
                }

                EventBuffer.PushEvent(eventMessage);
            }
        }
    }
}
