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

        public virtual IReadOnlyDictionary<string, DbContext> DbContexts { get; }

        public void ClearDbContexts()
        {
        }

        public virtual IQueryable<T> FromSqlInterpolated<T>(FormattableString sql) where T : class
        {
            throw new NotImplementedException();
        }

        public virtual IQueryable<T> FromSqlRaw<T>(string sql, params object[] parameters) where T : class
        {
            throw new NotImplementedException();
        }

        public virtual DbContext GetDbContext(string schemaSpace)
        {
            throw new NotImplementedException();
        }

        public virtual DbContext GetDbContext(Type entityType)
        {
            throw new NotImplementedException();
        }
    }
}
