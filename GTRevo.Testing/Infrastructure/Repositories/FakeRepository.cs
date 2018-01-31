using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Transactions;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Repositories;

namespace GTRevo.Testing.Infrastructure.Repositories
{
    public class FakeRepository : IRepository
    {
        private readonly List<SaveTransaction> saveTransactions = new List<SaveTransaction>();

        public FakeRepository()
        {
        }

        public List<EntityEntry> Aggregates { get; } = new List<EntityEntry>();
        public IReadOnlyList<SaveTransaction> SaveTransactions => saveTransactions;

        public ITransaction CreateTransaction()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public void Add<T>(T aggregate) where T : class, IAggregateRoot
        {
            if (!Aggregates.Any(x => x.Instance == aggregate))
            {
                Aggregates.Add(new EntityEntry(aggregate, EntityState.Added));
            }
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return GetSavedAggregates<T>().FirstOrDefault(predicate.Compile());
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return GetSavedAggregates<T>().First(predicate.Compile());
        }

        public Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return Task.FromResult(FirstOrDefault(predicate));
        }

        public Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return Task.FromResult(First(predicate));
        }

        public T Find<T>(Guid id) where T : class, IAggregateRoot
        {
            return Aggregates.Select(x => x.Instance).OfType<T>().FirstOrDefault(x => x.Id.Equals(id));
        }

        public Task<T> FindAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            return Task.FromResult(
                Aggregates.Select(x => x.Instance).OfType<T>().FirstOrDefault(x => x.Id.Equals(id)));
        }

        public T Get<T>(Guid id) where T : class, IAggregateRoot
        {
            return Aggregates.Select(x => x.Instance).OfType<T>().First(x => x.Id.Equals(id));
        }

        public Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            return Task.FromResult(Get<T>(id));
        }

        public IQueryable<T> FindAll<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return GetSavedAggregates<T>().AsQueryable();
        }

        public Task<IList<T>> FindAllAsync<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return Task.FromResult((IList<T>)FindAll<T>().ToList());
        }

        public IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return GetSavedAggregates<T>().Where(predicate.Compile()).AsQueryable();
        }

        public void Remove<T>(T aggregate) where T : class, IAggregateRoot
        {
            EntityEntry entry = Aggregates.FirstOrDefault(x => x.Instance == aggregate);
            if (entry != null)
            {
                entry.EntityState = EntityState.Removed;
            }
        }

        public void MarkModified<T>(T aggregate) where T : class, IAggregateRoot
        {
            EntityEntry entry = Aggregates.FirstOrDefault(x => x.Instance == aggregate);
            if (entry != null)
            {
                entry.EntityState = EntityState.Modified;
            }
        }

        public void SaveChanges()
        {
            List<EntityEntry> added = new List<EntityEntry>();
            List<EntityEntry> modified = new List<EntityEntry>();
            List<EntityEntry> removed = new List<EntityEntry>();
            MultiValueDictionary<EntityEntry, DomainAggregateEvent> publishedEvents = new MultiValueDictionary<EntityEntry, DomainAggregateEvent>();
            
            foreach (EntityEntry entry in Aggregates.ToList())
            {
                if (entry.EntityState == EntityState.Added || entry.EntityState == EntityState.Modified
                    || entry.Instance.UncommittedEvents.Any())
                {
                    if (entry.EntityState == EntityState.Added)
                    {
                        added.Add(entry);
                    }
                    else
                    {
                        modified.Add(entry);
                    }

                    entry.EntityState = EntityState.Unchanged;

                    foreach (DomainAggregateEvent domainEvent in entry.Instance.UncommittedEvents)
                    {
                        publishedEvents.Add(entry, domainEvent);
                    }

                    entry.Instance.Commit();
                }
                else if (entry.EntityState == EntityState.Removed)
                {
                    removed.Add(entry);
                    Aggregates.Remove(entry);
                }
            }

            saveTransactions.Add(
                new SaveTransaction(added, modified, removed, publishedEvents.AsLookup()));
        }

        public Task SaveChangesAsync()
        {
            SaveChanges();
            return Task.FromResult(0);
        }

        public IEnumerable<T> GetRemovedAggregates<T>() where T : class, IAggregateRoot
        {
            return SaveTransactions
                .SelectMany(x => x.Removed)
                .Select(x => x.Instance)
                .OfType<T>();
        }

        public bool HasUnsavedChanges()
        {
            return Aggregates
                .Any(x => x.Instance.UncommittedEvents.Any() || x.EntityState != EntityState.Unchanged);
        }

        protected IEnumerable<T> GetSavedAggregates<T>() where T : class, IAggregateRoot
        {
            return Aggregates
                .Where(x => x.EntityState == EntityState.Modified || x.EntityState == EntityState.Unchanged)
                .Select(x => x.Instance)
                .OfType<T>();
        }

        public enum EntityState
        {
            Added, Unchanged, Modified, Removed
        }

        public class EntityEntry
        {
            public EntityEntry(IAggregateRoot instance, EntityState entityState)
            {
                Instance = instance;
                EntityState = entityState;
            }

            public IAggregateRoot Instance { get; }
            public EntityState EntityState { get; set; }
            public int SaveCount { get; set; } = 0;
        }

        public class SaveTransaction
        {
            public SaveTransaction(IEnumerable<EntityEntry> added, IEnumerable<EntityEntry> modified,
                IEnumerable<EntityEntry> removed, ILookup<EntityEntry, DomainAggregateEvent> publishedEvents)
            {
                Added = added;
                Modified = modified;
                Removed = removed;
                PublishedEvents = publishedEvents;
            }

            public IEnumerable<EntityEntry> Added { get; }
            public IEnumerable<EntityEntry> Modified { get; }
            public IEnumerable<EntityEntry> Removed { get; }
            public ILookup<EntityEntry, DomainAggregateEvent> PublishedEvents { get; }
        }
    }
}
