using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GTRevo.Platform.Transactions;

namespace GTRevo.DataAccess.EF6
{
    public class Repository : IRepository, ITransactionProvider
    {
        private DbContext dbContext;

        public Repository(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Attach<T>(T entity) where T : class
        {
            dbContext.Set<T>().Attach(entity);
        }

        public void AttachRange<T>(IEnumerable<T> entities) where T : class
        {
            foreach (T entity in entities)
            {
                dbContext.Set<T>().Attach(entity);
            }
        }

        public void Add<T>(T entity) where T : class
        {
            dbContext.Set<T>().Add(entity);
        }

        public void AddRange<T>(IEnumerable<T> entities) where T : class
        {
            dbContext.Set<T>().AddRange(entities);
        }

        public T Get<T>(object id) where T : class
        {
            T t = dbContext.Set<T>().Find(id);
            ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public T Get<T>(params object[] id) where T : class
        {
            T t = dbContext.Set<T>().Find(id);
            ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(object[] id) where T : class
        {
            T t = await dbContext.Set<T>().FindAsync(id);
            ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(object id) where T : class
        {
            T t = await dbContext.Set<T>().FindAsync(id);
            ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return dbContext.Set<T>().First(predicate);
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return dbContext.Set<T>().First(predicate);
        }

        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await dbContext.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await dbContext.Set<T>().FirstAsync(predicate);
        }

        public IQueryable<T> FindAll<T>() where T : class
        {
            return dbContext.Set<T>();
        }
        
        public async Task<IList<T>> FindAllAsync<T>() where T : class
        {
            return await dbContext.Set<T>().ToListAsync();
        }

        public IEnumerable<T> FindAllWithAdded<T>() where T : class
        {
            var addedEntities = dbContext.ChangeTracker.Entries<T>()
                .Where(x => x.State == EntityState.Added)
                .Select(x => x.Entity);

            return dbContext.Set<T>()
                .Union(addedEntities);
        }

        public void Remove<T>(T entity) where T : class
        {
            if (IsAttached(entity))
            {
                dbContext.Set<T>().Remove(entity);
            }
        }

        public IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return dbContext.Set<T>().Where(predicate);
        }

        public IEnumerable<T> WhereWithAdded<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            IEnumerable<T> addedEntities = dbContext.ChangeTracker.Entries<T>()
                .Where(x => x.State == EntityState.Added)
                .Select(x => x.Entity)
                .Where(predicate.Compile());

            return dbContext.Set<T>().Where(predicate).ToList()
                .Union(addedEntities);
        }

        public bool IsAttached<T>(T entity) where T : class
        {
            return dbContext.Set<T>().Local.Any(x => x == entity);
        }

        public void SaveChanges()
        {
            dbContext.SaveChanges();
        }

        public Task SaveChangesAsync()
        {
            return dbContext.SaveChangesAsync();
        }

        public DbEntityEntry Entry(object entity)
        {
            return dbContext.Entry(entity);
        }

        public DbEntityEntry<T> Entry<T>(T entity) where T : class
        {
            return dbContext.Entry<T>(entity);
        }

        public ITransaction CreateTransaction()
        {
            return new RepositoryTransaction(this);
        }

        public void Dispose()
        {
            if (dbContext != null)
            {
                dbContext.Dispose();
            }
        }

        private void ThrowIfGetFailed<T>(T t, params object[] id)
        {
            if (t == null)
            {
                throw new ArgumentException($"{typeof(T).FullName} with ID '{string.Join(", ", id)}' was not found");
            }
        }
    }
}
