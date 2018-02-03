using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using Revo.DataAccess.Entities;

namespace Revo.DataAccess.EF6.Entities
{
    public interface IEF6ReadRepository : IReadRepository
    {
        IEnumerable<DbEntityEntry> Entries();
        DbEntityEntry Entry(object entity);
        DbEntityEntry<T> Entry<T>(T entity) where T : class;
    }
}
