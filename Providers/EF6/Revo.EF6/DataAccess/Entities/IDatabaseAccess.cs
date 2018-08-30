using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace Revo.EF6.DataAccess.Entities
{
    public interface IDatabaseAccess
    {
        Database Database { get; }

        void ExecuteProcedure(string procedureCommand, params object[] sqlParams);
        Task ExecuteProcedureAsync(string procedureCommand, params object[] sqlParams);
        DbRawSqlQuery SqlQuery(Type elementType, string sql, params object[] parameters);
        DbRawSqlQuery<T> SqlQuery<T>(string sql, params object[] parameters) where T : class;
        DbRawSqlQuery<T> SqlQueryNontracked<T>(string sql, params object[] parameters);
    }
}
