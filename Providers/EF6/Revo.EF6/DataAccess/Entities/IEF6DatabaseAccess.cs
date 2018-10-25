using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace Revo.EF6.DataAccess.Entities
{
    public interface IEF6DatabaseAccess
    {
        IReadOnlyDictionary<string, DbContext> DbContexts { get; }

        void ExecuteProcedure(string procedureCommand, string schemaSpace, params object[] sqlParams);
        Task ExecuteProcedureAsync(string procedureCommand, string schemaSpace, params object[] sqlParams);
        DbContext GetDbContext(string schemaSpace);
        DbContext GetDbContext(Type entityType);
        DbRawSqlQuery SqlQuery(Type elementType, string sql, params object[] parameters);
        DbRawSqlQuery<T> SqlQuery<T>(string sql, params object[] parameters) where T : class;
        DbRawSqlQuery<T> SqlQueryNontracked<T>(string sql, params object[] parameters);
    }
}
