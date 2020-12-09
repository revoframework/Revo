using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using Revo.DataAccess.Entities;

namespace Revo.EFCore.DataAccess.Entities
{
    public interface IEFCoreReadRepository : IReadRepository
    {
        IEFCoreDatabaseAccess DatabaseAccess { get; }

        IQueryable<T> FromSqlInterpolated<T>([NotParameterized] FormattableString sql)
            where T : class;
        IQueryable<T> FromSqlRaw<T>([NotParameterized] string sql, params object[] parameters)
            where T : class;
    }
}
