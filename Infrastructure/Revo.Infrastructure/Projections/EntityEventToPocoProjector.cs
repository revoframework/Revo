using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Revo.Core.Collections;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Projections
{
    public abstract class EntityEventToPocoProjector<TSource, TTarget> :
        IEntityEventProjector<TSource>
        where TSource : class, IEventSourcedAggregateRoot
    {
        private readonly MultiValueDictionary<Type, Func<IEventMessage<DomainAggregateEvent>, Task>> applyHandlers =
            new MultiValueDictionary<Type, Func<IEventMessage<DomainAggregateEvent>, Task>>();

        public EntityEventToPocoProjector()
        {
            CreateApplyDelegates();
        }

        public Type ProjectedAggregateType => typeof(TSource);
        
        protected Guid AggregateId { get; private set; }
        protected TTarget Target { get; private set; }

        public abstract Task CommitChangesAsync();
        protected abstract Task<TTarget> CreateProjectionTargetAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events);
        protected abstract Task<TTarget> GetProjectionTargetAsync(Guid aggregateId);
        
        public async Task ProjectEventsAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events)
        {
            if (events.Count == 0)
            {
                throw new InvalidOperationException($"No events to project for aggregate with ID {aggregateId}");
            }

            TTarget target;
            long firstEventNumber = events.First().Metadata.GetStreamSequenceNumber()
                                    ?? throw new InvalidOperationException(
                                        $"Cannot project events for aggregate with ID {aggregateId}, unknown StreamSequenceNumber for first events");
            if (firstEventNumber == 1)
            {
                target = await CreateProjectionTargetAsync(aggregateId, events);
            }
            else
            {
                target = await GetProjectionTargetAsync(aggregateId);
            }

            if (target == null)
            {
                return; //skip
            }

            AggregateId = aggregateId;
            Target = target;

            try
            {
                IEnumerable<IEventMessage<DomainAggregateEvent>> appliedEvents = events;

                var targetRowVersioned = target as IManuallyRowVersioned;
                if (targetRowVersioned != null) //refactor to derived type?
                {
                    appliedEvents = appliedEvents.SkipWhile(x =>
                        targetRowVersioned.Version >= (x.Metadata.GetStreamSequenceNumber() ?? 0));
                }

                appliedEvents = appliedEvents.ToList();
                if (appliedEvents.Any())
                {
                    await ApplyEvents(appliedEvents);

                    if (targetRowVersioned != null)
                    {
                        long newVersion = appliedEvents.Last().Metadata.GetStreamSequenceNumber()
                                          ?? targetRowVersioned.Version + appliedEvents.Count();
                        targetRowVersioned.Version = (int)newVersion;
                    }
                }
            }
            finally
            {
                AggregateId = default(Guid);
                Target = default(TTarget);
            }
        }

        protected void AddSubProjector(ISubEntityEventProjector projector)
        {
            CreateApplyDelegates(projector.GetType(), projector);
        }

        protected async Task ExecuteHandler<T>(T evt) where T : IEventMessage<DomainAggregateEvent>
        {
            IReadOnlyCollection<Func<IEventMessage<DomainAggregateEvent>, Task>> handlers;
            if (applyHandlers.TryGetValue(evt.Event.GetType(), out handlers))
            {
                foreach (var handler in handlers)
                {
                    await handler(evt);
                }
            }
        }

        private async Task ApplyEvents(IEnumerable<IEventMessage<DomainAggregateEvent>> events)
        {
            foreach (IEventMessage<DomainAggregateEvent> ev in events)
            {
                await ExecuteHandler(ev);
            }
        }

        private void CreateApplyDelegates()
        {
            CreateApplyDelegates(GetType(), this);
        }

        private void CreateApplyDelegates(Type projectorType, object instance)
        {
            if (projectorType.BaseType != null)
            {
                CreateApplyDelegates(projectorType.BaseType, instance);
            }

            var actions = projectorType
                .GetMethods(BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public |
                            BindingFlags.NonPublic)
                .Where(x => x.Name == "Apply"
                            && x.GetBaseDefinition() == x //exclude overrides
                            && x.GetParameters().Length == 1
                            && typeof(IEventMessage<DomainAggregateEvent>).IsAssignableFrom(x.GetParameters()[0].ParameterType))
                .Select(x => new Tuple<Type, Func<IEventMessage<DomainAggregateEvent>, Task>>(x.GetParameters()[0].ParameterType.GetGenericArguments()[0],
                    ev =>
                    {
                        Task ret = x.Invoke(instance, new object[] {ev}) as Task;
                        if (ret != null)
                        {
                            return ret;
                        }

                        return Task.FromResult(0);
                    }));

            actions = actions.Concat(
                projectorType
                    .GetMethods(BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public |
                                BindingFlags.NonPublic)
                    .Where(x => x.Name == "Apply"
                                && x.GetBaseDefinition() == x //exclude overrides
                                && x.GetParameters().Length == 3
                                && typeof(IEventMessage<DomainAggregateEvent>).IsAssignableFrom(x.GetParameters()[0].ParameterType)
                                && x.GetParameters()[1].ParameterType.IsAssignableFrom(typeof(Guid))
                                && x.GetParameters()[2].ParameterType.IsAssignableFrom(typeof(TTarget)))
                    .Select(x => new Tuple<Type, Func<IEventMessage<DomainAggregateEvent>, Task>>(x.GetParameters()[0].ParameterType.GetGenericArguments()[0],
                        ev =>
                        {
                            Task ret = x.Invoke(instance, new object[] {ev, this.AggregateId, this.Target}) as Task;
                            if (ret != null)
                            {
                                return ret;
                            }

                            return Task.FromResult(0);
                        })));
            
            foreach (var action in actions)
            {
                applyHandlers.Add(action.Item1, action.Item2);
            }
        }
    }
}
