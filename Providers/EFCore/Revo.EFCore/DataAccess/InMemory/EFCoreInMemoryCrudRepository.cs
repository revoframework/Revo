using System;
using System.Linq;
using Revo.DataAccess.InMemory;
using Revo.EFCore.DataAccess.Entities;

namespace Revo.EFCore.DataAccess.InMemory
{
    public class EFCoreInMemoryCrudRepository : InMemoryCrudRepository, IEFCoreCrudRepository
    {
        public IEFCoreDatabaseAccess DatabaseAccess { get; } = new EFCoreInMemoryDatabaseAccess();

        public virtual IQueryable<T> FromSqlInterpolated<T>(FormattableString sql) where T : class
        {
            throw new NotImplementedException();
        }

        public virtual IQueryable<T> FromSqlRaw<T>(string sql, params object[] parameters) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
