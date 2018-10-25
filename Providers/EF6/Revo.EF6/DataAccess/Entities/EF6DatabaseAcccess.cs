using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Revo.EF6.DataAccess.Model;

namespace Revo.EF6.DataAccess.Entities
{
    public class EF6DatabaseAccess : IEF6DatabaseAccess
    {
        private readonly Dictionary<string, DbContext> dbContexts = new Dictionary<string, DbContext>();
        private readonly IDbContextFactory dbContextFactory;
        private readonly IRequestDbContextCache requestDbContextCache;
        private readonly IModelMetadataExplorer modelMetadataExplorer;

        public EF6DatabaseAccess(IDbContextFactory dbContextFactory,
            IRequestDbContextCache requestDbContextCache,
            IModelMetadataExplorer modelMetadataExplorer)
        {
            this.dbContextFactory = dbContextFactory;
            this.requestDbContextCache = requestDbContextCache;
            this.modelMetadataExplorer = modelMetadataExplorer;
        }

        public IReadOnlyDictionary<string, DbContext> DbContexts => dbContexts;

        public void ExecuteProcedure(string procedureCommand, string schemaSpace, params object[] sqlParams)
        {
            var dbContext = GetDbContext(schemaSpace);
            dbContext.Database.ExecuteSqlCommand(procedureCommand, sqlParams);
        }

        public Task ExecuteProcedureAsync(string procedureCommand, string schemaSpace, params object[] sqlParams)
        {
            var dbContext = GetDbContext(schemaSpace);
            return dbContext.Database.ExecuteSqlCommandAsync(procedureCommand, sqlParams);
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
            string schemaSpace = modelMetadataExplorer.GetEntityTypeSchemaSpace(entityType);
            return GetDbContext(schemaSpace);
        }

        public DbRawSqlQuery SqlQuery(Type elementType, string sql, params object[] parameters)
        {
            var dbContext = GetDbContext(elementType);
            var objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;
            if (objectContext.MetadataWorkspace.GetEntityContainer(objectContext.DefaultContainerName, DataSpace.CSpace)
                .BaseEntitySets.Any(x => x.ElementType.Name == elementType.Name)) //TODO: better solution
                                                                                  //NOTE by MZ: ported from another project of mine, will this work with our custom conventions?
            {
                return dbContext.Set(elementType).SqlQuery(sql, parameters);
            }
            else
            {
                return dbContext.Database.SqlQuery(elementType, sql, parameters);
            }
        }

        public DbRawSqlQuery<T> SqlQuery<T>(string sql, params object[] parameters) where T : class
        {
            var dbContext = GetDbContext(typeof(T));
            var objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;
            if (objectContext.MetadataWorkspace.GetEntityContainer(objectContext.DefaultContainerName, DataSpace.CSpace)
                .BaseEntitySets.Any(x => x.ElementType.Name == typeof(T).Name)) //TODO: better solution
                                                                                //NOTE by MZ: ported from another project of mine, will this work with our custom conventions?
            {
                return dbContext.Set<T>().SqlQuery(sql, parameters);
            }
            else
            {
                return dbContext.Database.SqlQuery<T>(sql, parameters);
            }
        }

        public DbRawSqlQuery<T> SqlQueryNontracked<T>(string sql, params object[] parameters)
        {
            var dbContext = GetDbContext(typeof(T));
            return dbContext.Database.SqlQuery<T>(sql, parameters);
        }
    }
}
