using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GTRevo.DataAccess.Entities;

namespace GTRevo.DataAccess.EF6.Entities
{
    public interface IEF6ReadRepository : IReadRepository
    {
        IEnumerable<DbEntityEntry> Entries();
        DbEntityEntry Entry(object entity);
        DbEntityEntry<T> Entry<T>(T entity) where T : class;
    }
}
