using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Entities.Basic;
using Revo.Domain.Events;
using Revo.Domain.Tenancy;
using Revo.Infrastructure.Events;

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

        public virtual bool NeedsSave => crudRepository
            .GetEntities<object>(EntityState.Added, EntityState.Deleted, EntityState.Modified).Any();

        public void Add<T>(T aggregate) where T : class, IAggregateRoot
        {
            crudRepository.Add(aggregate);
        }
        
        public bool CanHandleAggregateType(Type aggregateType)
        {
            return aggregateType.GetCustomAttributes(typeof(DatabaseEntityAttribute), true).Any();
        }

        public T Find<T>(Guid id) where T : class, IAggregateRoot
        {
            return crudRepository.Find<T>(id);
        }

        public Task<T> FindAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            return crudRepository.FindAsync<T>(id);
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FirstOrDefault(predicate);
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.First(predicate);
        }

        public Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FirstOrDefaultAsync(predicate);
        }

        public Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FirstAsync(predicate);
        }

        public IQueryable<T> FindAll<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FindAll<T>();
        }

        public Task<IList<T>> FindAllAsync<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FindAllAsync<T>();
        }

        public async Task<IList<T>> FindAllAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return await crudRepository.Where<T>(predicate).ToListAsync(crudRepository);
        }

        public T Get<T>(Guid id) where T : class, IAggregateRoot
        {
            return crudRepository.Get<T>(id);
        }

        public Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            return crudRepository.GetAsync<T>(id);
        }

        public IEnumerable<IAggregateRoot> GetTrackedAggregates()
        {
            return crudRepository.GetEntities<IAggregateRoot>();
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
            await CommitAggregatesAsync();
            await crudRepository.SaveChangesAsync();
        }

        protected void InjectClassIds()
        {
            var addedClassEntitites =
                crudRepository.GetEntities<IBasicClassIdEntity>(Revo.DataAccess.Entities.EntityState.Added, Revo.DataAccess.Entities.EntityState.Modified);

            foreach (IBasicClassIdEntity entity in addedClassEntitites)
            {
                if (entity.ClassId == Guid.Empty)
                {
                    entity.ClassId = entityTypeManager.GetClassInfoByClrType(entity.GetType()).Id;
                }
            }
        }

        protected async Task CommitAggregatesAsync()
        {
            foreach (var aggregate in GetTrackedAggregates())
            {
                if (aggregate.IsChanged)
                {
                    var eventMessages = await CreateEventMessagesAsync(aggregate, aggregate.UncommittedEvents);
                    eventMessages.ForEach(publishEventBuffer.PushEvent);
                    aggregate.Commit();
                }
            }
        }

        private async Task<List<IEventMessageDraft>> CreateEventMessagesAsync(IAggregateRoot aggregate, IReadOnlyCollection<DomainAggregateEvent> events)
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

                messages.Add(message);
            }

            return messages;
        }
    }
}
