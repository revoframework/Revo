using System.Collections.Generic;
using Revo.DataAccess.Entities;

namespace Revo.EF6.DataAccess.Entities
{
    public interface IEF6ReadRepository : IReadRepository
    {
        IEF6DatabaseAccess DatabaseAccess { get; }
        IEnumerable<IDbEntityEntry> Entries();
        IDbEntityEntry Entry(object entity);
        IDbEntityEntry<T> Entry<T>(T entity) where T : class;
    }
}
