using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Revo.EFCore.DataAccess.Entities
{
    public interface IEFCoreDatabaseAccess : IDisposable
    {
        IReadOnlyDictionary<string, DbContext> DbContexts { get; }

        IQueryable<T> FromSql<T>([NotParameterized] FormattableString sql)
            where T : class;

        IQueryable<T> FromSql<T>([NotParameterized] RawSqlString sql,
            params object[] parameters)
            where T : class;

        DbContext GetDbContext(string schemaSpace);
        DbContext GetDbContext(Type entityType);
    }
}
