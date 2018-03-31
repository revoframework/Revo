using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.Entities;
using Revo.DataAccess.InMemory;

namespace Revo.DataAccess.EF6.InMemory
{
    public class EF6InMemoryCrudRepository : InMemoryCrudRepository, IEF6CrudRepository
    {
        public EF6InMemoryCrudRepository()
        {
        }

        public IEnumerable<IDbEntityEntry> Entries()
        {
            return EntityEntries.Select(x => new InMemoryDbEntityEntry(x));
        }

        public IDbEntityEntry Entry(object entity)
        {
            var entry = EntityEntries.FirstOrDefault(x => x.Instance == entity);
            if (entry == null)
            {
                EntityEntries.Add(new EntityEntry(entity, EntityState.Detached));
            }

            return new InMemoryDbEntityEntry(entry);
        }

        public IDbEntityEntry<T> Entry<T>(T entity) where T : class
        {
            var entry = EntityEntries.FirstOrDefault(x => x.Instance == entity);
            if (entry == null)
            {
                EntityEntries.Add(new EntityEntry(entity, EntityState.Detached));
            }

            return new InMemoryDbEntityEntry<T>(entry);
        }

        protected override IQueryable<T> CreateQueryable<T>(IEnumerable<T> enumerable)
        {
            return new LocalDbAsyncEnumerable<T>(enumerable);
        }
    }
}
