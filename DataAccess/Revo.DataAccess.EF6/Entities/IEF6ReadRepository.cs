using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using Revo.DataAccess.Entities;

namespace Revo.DataAccess.EF6.Entities
{
    public interface IEF6ReadRepository : IReadRepository
    {
        IEnumerable<IDbEntityEntry> Entries();
        IDbEntityEntry Entry(object entity);
        IDbEntityEntry<T> Entry<T>(T entity) where T : class;
    }
}
