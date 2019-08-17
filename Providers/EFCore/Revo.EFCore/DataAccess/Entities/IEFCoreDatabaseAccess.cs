using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Revo.EFCore.DataAccess.Entities
{
    public interface IEFCoreDatabaseAccess : IDisposable
    {
        IReadOnlyDictionary<string, DbContext> DbContexts { get; }

        void ClearDbContexts();

        IQueryable<T> FromSqlInterpolated<T>([NotParameterized] FormattableString sql)
            where T : class;
        IQueryable<T> FromSqlRaw<T>([NotParameterized] string sql, params object[] parameters)
            where T : class;

        DbContext GetDbContext(string schemaSpace);
        DbContext GetDbContext(Type entityType);
    }
}
