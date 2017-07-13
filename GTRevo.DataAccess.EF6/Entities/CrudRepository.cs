using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GTRevo.Core.Transactions;
using GTRevo.DataAccess.EF6.Model;
using GTRevo.DataAccess.Entities;

namespace GTRevo.DataAccess.EF6.Entities
{
    public class CrudRepository : ICrudRepository, ITransactionProvider
    {
        private readonly Dictionary<string, DbContext> dbContexts = new Dictionary<string, DbContext>();
        private readonly IDbContextFactory dbContextFactory;
        private readonly IModelMetadataExplorer modelMetadataExplorer;

        public CrudRepository(IModelMetadataExplorer modelMetadataExplorer,
            IDbContextFactory dbContextFactory)
        {
            this.modelMetadataExplorer = modelMetadataExplorer;
            this.dbContextFactory = dbContextFactory;
        }

        public void Attach<T>(T entity) where T : class
        {
            GetDbContext(typeof(T)).Set<T>().Attach(entity);
        }

        public void AttachRange<T>(IEnumerable<T> entities) where T : class
        {
            foreach (T entity in entities)
            {
                GetDbContext(typeof(T)).Set<T>().Attach(entity);
            }
        }

        public void Add<T>(T entity) where T : class
        {
            GetDbContext(typeof(T)).Set<T>().Add(entity);
        }

        public void AddRange<T>(IEnumerable<T> entities) where T : class
        {
            GetDbContext(typeof(T)).Set<T>().AddRange(entities);
        }

        public T Get<T>(object id) where T : class
        {
            T t = GetDbContext(typeof(T)).Set<T>().Find(id);
            RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public T Get<T>(params object[] id) where T : class
        {
            T t = GetDbContext(typeof(T)).Set<T>().Find(id);
            RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(object[] id) where T : class
        {
            T t = await GetDbContext(typeof(T)).Set<T>().FindAsync(id);
            RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(object id) where T : class
        {
            T t = await GetDbContext(typeof(T)).Set<T>().FindAsync(id);
            RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return GetDbContext(typeof(T)).Set<T>().FirstOrDefault(predicate);
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return GetDbContext(typeof(T)).Set<T>().First(predicate);
        }

        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await GetDbContext(typeof(T)).Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await GetDbContext(typeof(T)).Set<T>().FirstAsync(predicate);
        }

        public T Find<T>(object id) where T : class
        {
            T t = GetDbContext(typeof(T)).Set<T>().Find(id);
            return t;
        }

        public T Find<T>(params object[] id) where T : class
        {
            T t = GetDbContext(typeof(T)).Set<T>().Find(id);
            return t;
        }

        public async Task<T> FindAsync<T>(object[] id) where T : class
        {
            T t = await GetDbContext(typeof(T)).Set<T>().FindAsync(id);
            return t;
        }

        public async Task<T> FindAsync<T>(object id) where T : class
        {
            T t = await GetDbContext(typeof(T)).Set<T>().FindAsync(id);
            return t;
        }

        public IQueryable<T> FindAll<T>() where T : class
        {
            return GetDbContext(typeof(T)).Set<T>();
        }
        
        public async Task<IList<T>> FindAllAsync<T>() where T : class
        {
            return await GetDbContext(typeof(T)).Set<T>().ToListAsync();
        }

        public IEnumerable<T> FindAllWithAdded<T>() where T : class
        {
            var addedEntities = GetDbContext(typeof(T)).ChangeTracker.Entries<T>()
                .Where(x => x.State == EntityState.Added)
                .Select(x => x.Entity);

            return GetDbContext(typeof(T)).Set<T>()
                .Union(addedEntities);
        }

        public void Remove<T>(T entity) where T : class
        {
            if (IsAttached(entity))
            {
                GetDbContext(typeof(T)).Set<T>().Remove(entity);
            }
        }

        public IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return GetDbContext(typeof(T)).Set<T>().Where(predicate);
        }

        public IEnumerable<T> WhereWithAdded<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            IEnumerable<T> addedEntities = GetDbContext(typeof(T)).ChangeTracker.Entries<T>()
                .Where(x => x.State == EntityState.Added)
                .Select(x => x.Entity)
                .Where(predicate.Compile());

            return GetDbContext(typeof(T)).Set<T>().Where(predicate).ToList()
                .Union(addedEntities);
        }

        public EntityState GetEntityState<T>(T entity) where T : class
        {
            return GetDbContext(typeof(T)).Entry(entity).State;
        }

        public void SetEntityState<T>(T entity, EntityState state) where T : class
        {
            GetDbContext(typeof(T)).Entry(entity).State = state;
        }

        public IEnumerable<T> GetEntities<T>(params EntityState[] entityStates) where T : class
        {
            var entries = dbContexts.Values.SelectMany(x => x.ChangeTracker.Entries());
            if (entityStates?.Length > 0)
            {
                entries = entries.Where(x => entityStates.Any(s => (s & x.State) == s));
            }

            return entries.Select(x => x.Entity)
                .OfType<T>();
        }

        public bool IsAttached<T>(T entity) where T : class
        {
            return GetDbContext(typeof(T)).Set<T>().Local.Any(x => x == entity);
        }

        public void SaveChanges()
        {
            List<DbContext> modifiedDbContexts = dbContexts.Values.Where(x => x.ChangeTracker.HasChanges()).ToList();
            if (modifiedDbContexts.Count == 0)
            {
                return;
            }
            else if (modifiedDbContexts.Count > 1)
            {
                throw new InvalidOperationException(
                    "It is not allowed to modify entities from two or more different schema spaces at a time");
            }

            DbContext dbContext = modifiedDbContexts.First();
            IncrementEntityVersions(dbContext);
            try
            {
                dbContext.SaveChanges();
            }
            catch (Exception)
            {
                IncrementEntityVersions(dbContext, -1);
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            List<DbContext> modifiedDbContexts = dbContexts.Values.Where(x => x.ChangeTracker.HasChanges()).ToList();
            if (modifiedDbContexts.Count == 0)
            {
                return;
            }
            else if (modifiedDbContexts.Count > 1)
            {
                throw new InvalidOperationException(
                    "It is not allowed to modify entities from two or more different schema spaces at a time");
            }

            DbContext dbContext = modifiedDbContexts.First();
            IncrementEntityVersions(dbContext);
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                IncrementEntityVersions(dbContext, -1);
                throw;
            }
        }
        
        private void IncrementEntityVersions(DbContext dbContext, int increment = 1)
        {
            var changedEntities = dbContext
                .ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified)
                .Select(x => x.Entity)
                .OfType<IRowVersioned>();

            foreach (var entity in changedEntities)
            {
                entity.Version += increment;
            }
        }

        public IEnumerable<DbEntityEntry> Entries()
        {
            return dbContexts.Values.SelectMany(x => x.ChangeTracker.Entries());
        }

        public DbEntityEntry Entry(object entity)
        {
            return GetDbContext(entity.GetType()).Entry(entity);
        }

        public DbEntityEntry<T> Entry<T>(T entity) where T : class
        {
            return GetDbContext(typeof(T)).Entry<T>(entity);
        }

        public ITransaction CreateTransaction()
        {
            return new RepositoryTransaction(this);
        }

        public void Dispose()
        {
            foreach (var dbContext in dbContexts)
            {
                dbContext.Value.Dispose();
            }
        }

        private DbContext GetDbContext(string schemaSpace)
        {
            lock (this)
            {
                DbContext dbContext;
                if (dbContexts.TryGetValue(schemaSpace, out dbContext))
                {
                    return dbContext;
                }

                dbContext = dbContextFactory.CreateContext(schemaSpace);
                dbContexts.Add(schemaSpace, dbContext);
                return dbContext;
            }
        }

        private DbContext GetDbContext(Type entityType)
        {
            string schemaSpace = modelMetadataExplorer.GetEntityTypeSchemaSpace(entityType);
            return GetDbContext(schemaSpace);
        }
    }
}
