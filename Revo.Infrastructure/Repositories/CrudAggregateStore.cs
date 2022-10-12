using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Entities.Basic;
using Revo.Domain.Events;
using Revo.Domain.Tenancy;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventSourcing;

namespace Revo.Infrastructure.Repositories
{
    public class CrudAggregateStore : IQueryableAggregateStore
    {
        private readonly ICrudRepository crudRepository;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IPublishEventBuffer publishEventBuffer;
        private readonly IEventMessageFactory eventMessageFactory;

        public CrudAggregateStore(ICrudRepository crudRepository,
            IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer,
            IEventMessageFactory eventMessageFactory)
        {
            this.crudRepository = crudRepository;
            this.entityTypeManager = entityTypeManager;
            this.publishEventBuffer = publishEventBuffer;
            this.eventMessageFactory = eventMessageFactory;
        }

        public virtual bool NeedsSave => crudRepository.GetEntities<object>(
            EntityState.Added, EntityState.Deleted, EntityState.Modified).Any()
            || GetAttachedAggregates().Any(x => x.IsChanged || x.IsDeleted);

        public void Add<T>(T aggregate) where T : class, IAggregateRoot
        {
            crudRepository.Add(aggregate);
        }
        
        public bool CanHandleAggregateType(Type aggregateType)
        {
            return aggregateType.GetCustomAttributes(typeof(DatabaseEntityAttribute), true).Any();
        }

        public async Task<T> FindAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            return CheckAggregate(await crudRepository.FindAsync<T>(id), false);
        }
        
        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return CheckAggregate(await crudRepository.FirstOrDefaultAsync(predicate), false);
        }

        public async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return CheckAggregate(await crudRepository.FirstAsync(predicate), true);
        }

        public IQueryable<T> FindAll<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FindAll<T>();
        }

        public Task<T[]> FindAllAsync<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FindAllAsync<T>();
        }

        public async Task<T[]> FindAllAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return await crudRepository.Where<T>(predicate).ToArrayAsync(crudRepository);
        }

        public async Task<T[]> FindManyAsync<T>(params Guid[] ids) where T : class, IAggregateRoot
        {
            return (await crudRepository.FindManyAsync<T, Guid>(ids))
                .Where(x => CheckAggregate(x, false) != null)
                .ToArray();
        }

        public async Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            return CheckAggregate(await crudRepository.GetAsync<T>(id), true);
        }

        public async Task<T[]> GetManyAsync<T>(params Guid[] ids) where T : class, IAggregateRoot
        {
            var result = await crudRepository.GetManyAsync<T, Guid>(ids);
            foreach (var aggregate in result)
            {
                CheckAggregate(aggregate, true);
            }

            return result;
        }

        public IEnumerable<IAggregateRoot> GetTrackedAggregates()
        {
            return crudRepository.GetEntities<IAggregateRoot>();
        }

        public IEnumerable<IAggregateRoot> GetAttachedAggregates()
        {
            return crudRepository.GetEntities<IAggregateRoot>(
                EntityState.Added, EntityState.Modified, EntityState.Unchanged);
        }

        public IAsyncQueryableResolver GetQueryableResolver<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository;
        }
        
        public IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.Where(predicate);
        }

        public void Remove<T>(T aggregate) where T : class, IAggregateRoot
        {
            crudRepository.Remove(aggregate);
        }

        public virtual async Task SaveChangesAsync()
        {
            InjectClassIds();
            await PushAggregateEventsAsync();
            RemoveDeletedEntities();
            CommitAggregates();
            await crudRepository.SaveChangesAsync();
        }

        protected void InjectClassIds()
        {
            var addedClassEntitites = crudRepository.GetEntities<IBasicClassIdEntity>(
                EntityState.Added, EntityState.Modified)
                .Where(x => crudRepository.GetEntityState(x) != EntityState.Deleted && crudRepository.GetEntityState(x) != EntityState.Detached);

            foreach (IBasicClassIdEntity entity in addedClassEntitites)
            {
                if (entity.ClassId == Guid.Empty)
                {
                    entity.ClassId = entityTypeManager.GetClassInfoByClrType(entity.GetType()).Id;
                }
            }
        }

        protected void RemoveDeletedEntities()
        {
            foreach (var aggregate in GetAttachedAggregates().ToArray())
            {
                if (aggregate.IsDeleted)
                {
                    crudRepository.Remove((dynamic) aggregate);
                }
            }
        }

        protected async Task PushAggregateEventsAsync()
        {
            if (publishEventBuffer == null)
            {
                return;
            }

            var utcNow = Clock.Current.UtcNow;

            foreach (var aggregate in GetAttachedAggregates())
            {
                if (aggregate.IsChanged)
                {
                    var eventMessages = await CreateEventMessagesAsync(aggregate, aggregate.UncommittedEvents, utcNow); 
                    eventMessages.ForEach(publishEventBuffer.PushEvent);
                }
            }
        }

        protected void CommitAggregates()
        {
            foreach (var aggregate in GetAttachedAggregates())
            {
                if (aggregate.IsChanged)
                {
                    aggregate.Commit();
                }
            }
        }

        private T CheckAggregate<T>(T aggregate, bool throwOnError) where T : class, IAggregateRoot
        {
            if (aggregate != null && aggregate.IsDeleted)
            {
                if (throwOnError)
                {
                    throw new EntityDeletedException(
                        $"Cannot get {aggregate} because it has been previously deleted");
                }

                return null;
            }

            return aggregate;
        }

        private async Task<List<IEventMessageDraft>> CreateEventMessagesAsync(IAggregateRoot aggregate,
            IReadOnlyCollection<DomainAggregateEvent> events, DateTimeOffset utcNow)
        {
            var messages = new List<IEventMessageDraft>();
            Guid? aggregateClassId = entityTypeManager.TryGetClassInfoByClrType(aggregate.GetType())?.Id;

            foreach (DomainAggregateEvent ev in events)
            {
                IEventMessageDraft message = await eventMessageFactory.CreateMessageAsync(ev);
                if (aggregateClassId != null)
                {
                    message.SetMetadata(BasicEventMetadataNames.AggregateClassId, aggregateClassId.Value.ToString());
                }
                
                if (aggregate is ITenantOwned tenantOwned)
                {
                    message.SetMetadata(BasicEventMetadataNames.AggregateTenantId, tenantOwned.TenantId?.ToString());
                }

                if (message.Metadata.GetEventId() == null)
                {
                    message.SetMetadata(BasicEventMetadataNames.EventId, Guid.NewGuid().ToString());
                }

                message.SetMetadata(BasicEventMetadataNames.AggregateVersion, (aggregate.Version + 1).ToString());
                message.SetMetadata(BasicEventMetadataNames.StoreDate, utcNow.ToString(CultureInfo.InvariantCulture));
                
                messages.Add(message);
            }

            return messages;
        }
    }
}
