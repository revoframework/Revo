using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Revo.EFCore.DataAccess.Entities
{
    public class EFCoreDatabaseAccess : IEFCoreDatabaseAccess
    {
        private readonly Dictionary<string, DbContext> dbContexts = new Dictionary<string, DbContext>();
        private readonly IDbContextFactory dbContextFactory;

        public EFCoreDatabaseAccess(IDbContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public IReadOnlyDictionary<string, DbContext> DbContexts => dbContexts;

        public void Dispose()
        {
            foreach (var dbContext in dbContexts)
            {
                dbContext.Value.Dispose();
            }
        }
        public IQueryable<T> FromSql<T>([NotParameterized] FormattableString sql)
            where T : class
        {
            return GetDbContext(typeof(T)).Set<T>().FromSql(sql);
        }

        public IQueryable<T> FromSql<T>([NotParameterized] RawSqlString sql,
            params object[] parameters)
            where T : class
        {
            return GetDbContext(typeof(T)).Set<T>().FromSql(sql, parameters);
        }

        public DbContext GetDbContext(string schemaSpace)
        {
            lock (this)
            {
                DbContext dbContext;
                if (dbContexts.TryGetValue(schemaSpace, out dbContext))
                {
                    return dbContext;
                }

                dbContext = dbContextFactory.CreateContext(schemaSpace);
                dbContexts.Add(schemaSpace, dbContext);
                return dbContext;
            }
        }

        public DbContext GetDbContext(Type entityType)
        {
            string schemaSpace = "Default"; //modelMetadataExplorer.GetEntityTypeSchemaSpace(entityType);
            return GetDbContext(schemaSpace);
        }
    }
}
