using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GTRevo.DataAccess.EF6;
using GTRevo.Infrastructure.Security.Commands;

namespace GTRevo.Infrastructure.Security
{
    public class AuthorizingRepository : IAuthorizingRepository
    {
        private readonly IRepository repository;
        private readonly IEntityQueryAuthorizer entityQueryAuthorizer;

        public AuthorizingRepository(IRepository repository,
            IEntityQueryAuthorizer entityQueryAuthorizer)
        {
            this.repository = repository;
            this.entityQueryAuthorizer = entityQueryAuthorizer;
        }

        public void Dispose()
        {
            repository.Dispose();
        }

        public void Attach<T>(T entity) where T : class
        {
            repository.Attach(entity);
        }

        public void AttachRange<T>(IEnumerable<T> entities) where T : class
        {
            repository.AttachRange(entities);
        }

        public void Add<T>(T entity) where T : class
        {
            repository.Add(entity);
        }

        public void AddRange<T>(IEnumerable<T> entities) where T : class
        {
            repository.AddRange(entities);
        }

        public T Get<T>(object[] id) where T : class
        {
            return repository.Get<T>(id);
        }

        public T Get<T>(object id) where T : class
        {
            return repository.Get<T>(id);
        }

        public Task<T> GetAsync<T>(params object[] id) where T : class
        {
            return repository.GetAsync<T>(id);
        }

        public Task<T> GetAsync<T>(object id) where T : class
        {
            return repository.GetAsync<T>(id);
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return repository.FirstOrDefault(predicate);
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return repository.First(predicate);
        }

        public Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return repository.FirstOrDefaultAsync(predicate);
        }

        public Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return repository.FirstAsync(predicate);
        }

        public IQueryable<T> FindAll<T>() where T : class
        {
            return WrapQueryable(repository.FindAll<T>());
        }

        public Task<IList<T>> FindAllAsync<T>() where T : class
        {
            return repository.FindAllAsync<T>();
        }

        public IEnumerable<T> FindAllWithAdded<T>() where T : class
        {
            return repository.FindAllWithAdded<T>();
        }

        public void Remove<T>(T entity) where T : class
        {
            repository.Remove(entity);
        }

        public IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return WrapQueryable(repository.Where(predicate));
        }

        public IEnumerable<T> WhereWithAdded<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return repository.WhereWithAdded(predicate);
        }

        public DbEntityEntry Entry(object entity)
        {
            return repository.Entry(entity);
        }

        public DbEntityEntry<T> Entry<T>(T entity) where T : class
        {
            return repository.Entry(entity);
        }

        public bool IsAttached<T>(T entity) where T : class
        {
            return repository.IsAttached(entity);
        }

        public void SaveChanges()
        {
            repository.SaveChanges();
        }

        public Task SaveChangesAsync()
        {
            return repository.SaveChangesAsync();
        }

        private IQueryable<T> WrapQueryable<T>(IQueryable<T> query)
        {
            var queryProvider = new AuthorizingQueryProvider(query.Provider, entityQueryAuthorizer);

            //return queryProvider.CreateQuery<T>(query.Expression);

            DbQuery<T> dbQuery = query as DbQuery<T>;
            if (dbQuery != null)
            {
                var providerField = typeof(DbQuery<T>).GetField("_provider", BindingFlags.Instance | BindingFlags.NonPublic);
                providerField.SetValue(dbQuery, queryProvider);
            }

            return query;
        }
    }
}
