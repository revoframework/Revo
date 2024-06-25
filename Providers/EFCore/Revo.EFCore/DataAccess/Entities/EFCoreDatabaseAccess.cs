using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Revo.EFCore.DataAccess.Entities
{
    public class EFCoreDatabaseAccess : IEFCoreDatabaseAccess
    {
        public static readonly string DefaultSchemaSpace = "Default";

        private readonly Dictionary<string, DbContext> dbContexts = new Dictionary<string, DbContext>();
        private readonly IDbContextFactory dbContextFactory;
        private readonly IRequestDbContextCache requestDbContextCache;        

        public EFCoreDatabaseAccess(IDbContextFactory dbContextFactory, IRequestDbContextCache requestDbContextCache)
        {
            this.dbContextFactory = dbContextFactory;
            this.requestDbContextCache = requestDbContextCache;
        }

        public IReadOnlyDictionary<string, DbContext> DbContexts => dbContexts;

        public void ClearDbContexts()
        {
            dbContexts.Clear();
        }

        public void Dispose()
        {
        }

        public IQueryable<T> FromSqlInterpolated<T>([NotParameterized] FormattableString sql)
            where T : class
        {
            var dbContext = GetDbContext(typeof(T));
            return GetDbContext(typeof(T)).Set<T>().FromSqlInterpolated(sql);
        }

        public IQueryable<T> FromSqlRaw<T>([NotParameterized] string sql,
            params object[] parameters)
            where T : class
        {
            return GetDbContext(typeof(T)).Set<T>().FromSqlRaw(sql, parameters);
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
                requestDbContextCache.AddDbContext(dbContext);
                return dbContext;
            }
        }

        public DbContext GetDbContext(Type entityType)
        {
            string schemaSpace = DefaultSchemaSpace; //modelMetadataExplorer.GetEntityTypeSchemaSpace(entityType);
            return GetDbContext(schemaSpace);
        }
    }
}
