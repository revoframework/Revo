using System;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace GTRevo.DataAccess.EF6
{
    public class DatabaseAccess : IDatabaseAccess
    {
        private DbContext dbContext;

        public DatabaseAccess(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Database Database
        {
            get
            {
                return dbContext.Database;
            }
        }

        public void ExecuteProcedure(string procedureCommand, params object[] sqlParams)
        {
            dbContext.Database.ExecuteSqlCommand(procedureCommand, sqlParams);
        }

        public Task ExecuteProcedureAsync(string procedureCommand, params object[] sqlParams)
        {
            return dbContext.Database.ExecuteSqlCommandAsync(procedureCommand, sqlParams);
        }

        public DbRawSqlQuery SqlQuery(Type elementType, string sql, params object[] parameters)
        {
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
            return dbContext.Database.SqlQuery<T>(sql, parameters);
        }
    }
}
