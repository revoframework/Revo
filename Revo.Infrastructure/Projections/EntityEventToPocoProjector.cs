using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Projections
{
    /// <summary>
    /// An event projector for an aggregate type with a single POCO read model for every aggregate.
    /// A convention-based abstract base class that calls an Apply for every event type
    /// and also supports sub-projectors.
    /// </summary>
    /// <remarks>
    /// If TTarget is IManuallyRowVersioned, automatically handles read model versioning and projection
    /// idempotency using event sequence numbers.
    /// </remarks>
    /// <typeparam name="TSource">Aggregate type.</typeparam>
    /// <typeparam name="TTarget">Read model type.</typeparam>
    public abstract class EntityEventToPocoProjector<TTarget> : EntityEventProjector
    {
        /// <summary>
        /// Currently projected read-model instance.
        /// </summary>
        protected TTarget Target { get; private set; }

        /// <summary>
        /// Override to create a new TTarget instance.
        /// Called when the sequence number of the first projected event is 1, i.e. typically for newly created aggregates.
        /// </summary>
        /// <returns></returns>
        protected abstract Task<TTarget> CreateProjectionTargetAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events);

        /// <summary>
        /// Override to get an existing TTarget instance.
        /// Called when the sequence number of the first projected event is >1, i.e. typically for updated aggregates.
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <returns></returns>
        protected abstract Task<TTarget> GetProjectionTargetAsync(Guid aggregateId);
        
        public override async Task ProjectEventsAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events)
        {
            if (events.Count == 0)
            {
                throw new InvalidOperationException($"No events to project for aggregate with ID {aggregateId}");
            }

            TTarget target;
            long firstEventNumber = events.First().Metadata.GetStreamSequenceNumber()
                                    ?? throw new InvalidOperationException(
                                        $"Cannot project events for aggregate with ID {aggregateId}, unknown StreamSequenceNumber for first batch of events");
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
            
            Target = target;
            
            var appliedEvents = events;

            var targetRowVersioned = target as IManuallyRowVersioned;
            if (targetRowVersioned != null)
            {
                appliedEvents = appliedEvents
                    .SkipWhile(x =>
                        x.Metadata.GetStreamSequenceNumber().HasValue && targetRowVersioned.Version >= x.Metadata.GetStreamSequenceNumber())
                    .ToList();
            }
            
            try
            {
                if (appliedEvents.Any())
                {
                    await base.ProjectEventsAsync(aggregateId, appliedEvents);

                    if (targetRowVersioned != null)
                    {
                        long newVersion = appliedEvents.LastOrDefault()?.Metadata?.GetStreamSequenceNumber()
                                          ?? targetRowVersioned.Version + appliedEvents.Count();
                        targetRowVersioned.Version = (int)newVersion;
                    }
                }
            }
            finally
            {
                Target = default(TTarget);
            }
        }
        
        protected override IEnumerable<(Type EventType, Func<IEventMessage<DomainAggregateEvent>, Task> Delegate)> GetApplyDelegates(
            Type projectorType, object instance)
        {
            var actions = base.GetApplyDelegates(projectorType, instance);

            actions = actions.Concat(
                projectorType
                    .GetMethods(BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public |
                                BindingFlags.NonPublic)
                    .Where(x => x.Name == "Apply"
                                && x.GetBaseDefinition() == x //exclude overrides
                                && x.GetParameters().Length == 3
                                && typeof(IEventMessage<DomainAggregateEvent>).IsAssignableFrom(x.GetParameters()[0]
                                    .ParameterType)
                                && x.GetParameters()[1].ParameterType.IsAssignableFrom(typeof(Guid))
                                && x.GetParameters()[2].ParameterType.IsAssignableFrom(typeof(TTarget)))
                    .Select(x =>
                        (
                            EventType: x.GetParameters()[0].ParameterType.GetGenericArguments()[0],
                            Delegate: (Func<IEventMessage<DomainAggregateEvent>, Task>) (ev =>
                            {
                                Task ret = x.Invoke(instance, new object[] {ev, this.AggregateId, this.Target}) as Task;
                                if (ret != null)
                                {
                                    return ret;
                                }

                                return Task.FromResult(0);
                            })
                         )
                    ));

            return actions;
        }
    }
}
