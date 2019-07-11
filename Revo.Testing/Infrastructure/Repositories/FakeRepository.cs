using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Collections;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.Repositories;

namespace Revo.Testing.Infrastructure.Repositories
{
    public class FakeRepository : IRepository
    {
        private static readonly FakeAsyncQueryableResolver AsyncQueryableResolver = new FakeAsyncQueryableResolver();

        private readonly List<SaveTransaction> saveTransactions = new List<SaveTransaction>();

        public FakeRepository()
        {
        }

        public virtual List<EntityEntry> Aggregates { get; } = new List<EntityEntry>();
        public virtual IReadOnlyList<SaveTransaction> SaveTransactions => saveTransactions;

        public virtual ITransaction CreateTransaction()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public virtual void Add<T>(T aggregate) where T : class, IAggregateRoot
        {
            if (!Aggregates.Any(x => x.Instance == aggregate))
            {
                Aggregates.Add(new EntityEntry(aggregate, EntityState.Added));
            }
        }

        public virtual T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return GetSavedAggregates<T>().FirstOrDefault(predicate.Compile());
        }

        public virtual T First<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return GetSavedAggregates<T>().First(predicate.Compile());
        }

        public virtual Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return Task.FromResult(FirstOrDefault(predicate));
        }

        public virtual Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return Task.FromResult(First(predicate));
        }

        public virtual T Find<T>(Guid id) where T : class, IAggregateRoot
        {
            return Aggregates.Select(x => x.Instance).OfType<T>().FirstOrDefault(x => x.Id.Equals(id));
        }

        public virtual Task<T> FindAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            return Task.FromResult(
                Aggregates.Select(x => x.Instance).OfType<T>().FirstOrDefault(x => x.Id.Equals(id)));
        }
        
        public virtual IQueryable<T> FindAll<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return GetSavedAggregates<T>().AsQueryable();
        }

        public virtual Task<T[]> FindAllAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return Task.FromResult(Where(predicate).ToArray());
        }

        public virtual Task<T[]> FindAllAsync<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return Task.FromResult(FindAll<T>().ToArray());
        }

        public virtual Task<T[]> FindManyAsync<T>(params Guid[] ids) where T : class, IAggregateRoot
        {
            var result = new List<T>();
            foreach (Guid id in ids)
            {
                var aggregate = Aggregates.Select(x => x.Instance).OfType<T>().FirstOrDefault(x => x.Id.Equals(id));
                if (aggregate != null)
                {
                    result.Add(aggregate);
                }
            }

            return Task.FromResult(result.ToArray());
        }

        public virtual T Get<T>(Guid id) where T : class, IAggregateRoot
        {
            return Aggregates.Select(x => x.Instance).OfType<T>().First(x => x.Id.Equals(id));
        }

        public virtual Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            return Task.FromResult(Get<T>(id));
        }

        public virtual Task<T[]> GetManyAsync<T>(params Guid[] ids) where T : class, IAggregateRoot
        {
            var result = new List<T>();
            foreach (Guid id in ids)
            {
                var aggregate = Get<T>(id);
                result.Add(aggregate);
            }

            return Task.FromResult(result.ToArray());
        }

        public virtual IAsyncQueryableResolver GetQueryableResolver<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return AsyncQueryableResolver;
        }

        public virtual IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return GetSavedAggregates<T>().Where(predicate.Compile()).AsQueryable();
        }

        public virtual void Remove<T>(T aggregate) where T : class, IAggregateRoot
        {
            EntityEntry entry = Aggregates.FirstOrDefault(x => x.Instance == aggregate);
            if (entry != null)
            {
                entry.EntityState = EntityState.Removed;
            }
        }

        public virtual void MarkModified<T>(T aggregate) where T : class, IAggregateRoot
        {
            EntityEntry entry = Aggregates.FirstOrDefault(x => x.Instance == aggregate);
            if (entry != null)
            {
                entry.EntityState = EntityState.Modified;
            }
        }

        public virtual void SaveChanges()
        {
            List<EntityEntry> added = new List<EntityEntry>();
            List<EntityEntry> modified = new List<EntityEntry>();
            List<EntityEntry> removed = new List<EntityEntry>();
            MultiValueDictionary<EntityEntry, DomainAggregateEvent> publishedEvents = new MultiValueDictionary<EntityEntry, DomainAggregateEvent>();
            
            foreach (EntityEntry entry in Aggregates.ToList())
            {
                if (entry.EntityState == EntityState.Added || entry.EntityState == EntityState.Modified
                    || entry.Instance.IsChanged || entry.Instance.IsDeleted)
                {
                    if (entry.EntityState == EntityState.Added)
                    {
                        added.Add(entry);
                    }
                    else if (entry.EntityState == EntityState.Modified || entry.Instance.IsChanged)
                    {
                        modified.Add(entry);
                    }

                    entry.EntityState = EntityState.Unchanged;

                    foreach (DomainAggregateEvent domainEvent in entry.Instance.UncommittedEvents)
                    {
                        publishedEvents.Add(entry, domainEvent);
                    }

                    if (entry.Instance.IsDeleted)
                    {
                        entry.EntityState = EntityState.Removed;
                    }
                    else
                    {
                        entry.Instance.Commit();
                    }
                }

                if (entry.EntityState == EntityState.Removed)
                {
                    removed.Add(entry);
                    Aggregates.Remove(entry);
                }
            }

            saveTransactions.Add(
                new SaveTransaction(added, modified, removed,
                    publishedEvents
                        .SelectMany(x => x.Value.Select(y => new KeyValuePair<EntityEntry, DomainAggregateEvent>(x.Key, y)))
                        .ToLookup(x => x.Key, x => x.Value)));
        }

        public virtual Task SaveChangesAsync()
        {
            SaveChanges();
            return Task.FromResult(0);
        }

        public virtual IEnumerable<T> GetRemovedAggregates<T>() where T : class, IAggregateRoot
        {
            return SaveTransactions
                .SelectMany(x => x.Removed)
                .Select(x => x.Instance)
                .OfType<T>();
        }

        public virtual bool HasUnsavedChanges()
        {
            return Aggregates
                .Any(x => x.Instance.UncommittedEvents.Any() || x.EntityState != EntityState.Unchanged);
        }

        protected virtual IEnumerable<T> GetSavedAggregates<T>() where T : class, IAggregateRoot
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

        public class FakeAsyncQueryableResolver : IAsyncQueryableResolver
        {
            public Task<bool> AnyAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(queryable.Any());
            }

            public Task<int> CountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(queryable.Count());
            }

            public IQueryable<T> Include<T, TProperty>(IQueryable<T> queryable, Expression<Func<T, TProperty>> navigationPropertyPath) where T : class
            {
                return queryable;
            }

            public IQueryable<T> Include<T>(IQueryable<T> queryable, string navigationPropertyPath) where T : class
            {
                return queryable;
            }

            public Task<long> LongCountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(queryable.LongCount());
            }

            public Task<T> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(queryable.FirstOrDefault());
            }

            public Task<T> FirstAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(queryable.First());
            }

            public Task<T[]> ToArrayAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(queryable.ToArray());
            }

            public Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(queryable.ToList());
            }

            public Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(IQueryable<T> queryable, Func<T, TKey> keySelector,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(queryable.ToDictionary(keySelector));
            }

            public Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(IQueryable<T> queryable, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(queryable.ToDictionary(keySelector, comparer));
            }

            public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(IQueryable<T> queryable, Func<T, TKey> keySelector, Func<T, TElement> elementSelector,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(queryable.ToDictionary(keySelector, elementSelector));
            }

            public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(IQueryable<T> queryable, Func<T, TKey> keySelector, Func<T, TElement> elementSelector,
                IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(queryable.ToDictionary(keySelector, elementSelector, comparer));
            }
        }
    }
}
