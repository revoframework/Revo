using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;

namespace GTRevo.Infrastructure.Projections
{
    public abstract class EntityEventToPocoProjector<TSource, TTarget> :
        IEntityEventProjector<TSource>
        where TSource : class, IEventSourcedAggregateRoot
    {
        private readonly MultiValueDictionary<Type, Func<DomainAggregateEvent, Task>> applyHandlers = new MultiValueDictionary<Type, Func<DomainAggregateEvent, Task>>();

        public EntityEventToPocoProjector()
        {
            CreateApplyDelegates();
        }

        public Type ProjectedAggregateType
        {
            get { return typeof(TSource); }
        }

        protected TSource Aggregate { get; private set; }
        protected TTarget Target { get; private set; }

        public abstract Task CommitChangesAsync();
        protected abstract Task<TTarget> CreateProjectionTargetAsync(TSource aggregate, IEnumerable<DomainAggregateEvent> events);
        protected abstract Task<TTarget> GetProjectionTargetAsync(TSource aggregate);

        public Task ProjectEventsAsync(IEventSourcedAggregateRoot aggregate, IEnumerable<DomainAggregateEvent> events)
        {
            TSource source = aggregate as TSource;
            if (source == null)
            {
                throw new ArgumentException($"Invalid aggregate type for projection: {aggregate?.GetType().FullName ?? "null"}");
            }

            return ProjectEventsAsync(source, events);
        }

        public async Task ProjectEventsAsync(TSource aggregate, IEnumerable<DomainAggregateEvent> events)
        {
            if (aggregate.Version < 1)
            {
                throw new ArgumentException(
                    "Unexpected version of aggregate to project: should only project after savig its state, i.e. Version >= 1");
            }

            TTarget target;
            if (aggregate.Version == 1)
            {
                target = await CreateProjectionTargetAsync(aggregate, events);
            }
            else
            {
                target = await GetProjectionTargetAsync(aggregate);
            }

            Aggregate = aggregate;
            Target = target;

            try
            {
                await ApplyEvents(events);
            }
            finally
            {
                Aggregate = default(TSource);
                Target = default(TTarget);
            }
        }

        protected void AddSubProjector(ISubEntityEventProjector projector)
        {
            CreateApplyDelegates(projector.GetType(), projector);
        }

        protected async Task ExecuteHandler<T>(T evt) where T : DomainAggregateEvent
        {
            IReadOnlyCollection<Func<DomainAggregateEvent, Task>> handlers;
            if (applyHandlers.TryGetValue(evt.GetType(), out handlers))
            {
                foreach (var handler in handlers)
                {
                    await handler(evt);
                }
            }
        }

        private async Task ApplyEvents(IEnumerable<DomainAggregateEvent> events)
        {
            foreach (DomainAggregateEvent ev in events)
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
                            && typeof(DomainAggregateEvent).IsAssignableFrom(x.GetParameters()[0].ParameterType))
                .Select(x => new Tuple<Type, Func<DomainAggregateEvent, Task>>(x.GetParameters()[0].ParameterType,
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
                                && typeof(DomainAggregateEvent).IsAssignableFrom(x.GetParameters()[0].ParameterType)
                                && x.GetParameters()[1].ParameterType.IsAssignableFrom(typeof(TSource))
                                && x.GetParameters()[2].ParameterType.IsAssignableFrom(typeof(TTarget)))
                    .Select(x => new Tuple<Type, Func<DomainAggregateEvent, Task>>(x.GetParameters()[0].ParameterType,
                        ev =>
                        {
                            Task ret = x.Invoke(instance, new object[] {ev, this.Aggregate, this.Target}) as Task;
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
