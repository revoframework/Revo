using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Revo.EFCore.DataAccess.Entities;

namespace Revo.EFCore.DataAccess.InMemory
{
    public class EFCoreInMemoryDatabaseAccess : IEFCoreDatabaseAccess
    {
        public void Dispose()
        {
        }

        public IReadOnlyDictionary<string, DbContext> DbContexts { get; }
        public void ClearDbContexts()
        {
        }

        public IQueryable<T> FromSql<T>(FormattableString sql) where T : class
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> FromSql<T>(RawSqlString sql, params object[] parameters) where T : class
        {
            throw new NotImplementedException();
        }

        public DbContext GetDbContext(string schemaSpace)
        {
            throw new NotImplementedException();
        }

        public DbContext GetDbContext(Type entityType)
        {
            throw new NotImplementedException();
        }
    }
}
