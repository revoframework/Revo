using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using GTRevo.DataAccess.EF6;

namespace GTRevo.Testing.DataAccess.EF6
{
    public class FakeCrudRepository : ICrudRepository
    {
        private readonly List<object> entities = new List<object>();

        public FakeCrudRepository()
        {
        }

        public T Get<T>(object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public T Get<T>(object id) where T : class
        {
            return entities.OfType<T>().First(x => HasEntityId(x, id));
        }

        public Task<T> GetAsync<T>(params object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(object id) where T : class
        {
            return Task.FromResult(entities.OfType<T>().First(x => HasEntityId(x, id)));
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return entities.OfType<T>().FirstOrDefault(predicate.Compile());
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return entities.OfType<T>().First(predicate.Compile());
        }

        public Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return Task.FromResult(entities.OfType<T>().FirstOrDefault(predicate.Compile()));
        }

        public Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return Task.FromResult(entities.OfType<T>().First(predicate.Compile()));
        }

        public IQueryable<T> FindAll<T>() where T : class
        {
            return new LocalDbAsyncEnumerable<T>(entities.OfType<T>());
        }

        public Task<IList<T>> FindAllAsync<T>() where T : class
        {
            return Task.FromResult((IList<T>)entities.OfType<T>().ToList());
        }

        public IEnumerable<T> FindAllWithAdded<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return new LocalDbAsyncEnumerable<T>(entities.OfType<T>()).Where(predicate);
        }

        public IEnumerable<T> WhereWithAdded<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbEntityEntry> Entries()
        {
            throw new NotImplementedException();
        }

        public DbEntityEntry Entry(object entity)
        {
            throw new NotImplementedException();
        }

        public DbEntityEntry<T> Entry<T>(T entity) where T : class
        {
            throw new NotImplementedException();
        }

        public bool IsAttached<T>(T entity) where T : class
        {
            return entities.Contains(entity);
        }

        public void Attach<T>(T entity) where T : class
        {
            Add(entity);
        }

        public void AttachRange<T>(IEnumerable<T> entities) where T : class
        {
            AddRange(entities);
        }

        public void Add<T>(T entity) where T : class
        {
            if (!entities.Contains(entity))
            {
                entities.Add(entity);
            }
        }

        public void AddRange<T>(IEnumerable<T> entities) where T : class
        {
            foreach (T entity in entities)
            {
                Add(entity);
            }
        }

        public void Remove<T>(T entity) where T : class
        {
            entities.Remove(entity);
        }

        public void SaveChanges()
        {
        }

        public async Task SaveChangesAsync()
        {
        }

        public void Dispose()
        {
        }

        private object GetEntityId(object entity)
        {
            return entity.GetType().GetProperty("Id").GetValue(entity);
        }

        private bool HasEntityId(object entity, object id)
        {
            object entityId = GetEntityId(entity);
            return entityId.Equals(id);
        }
    }
}
