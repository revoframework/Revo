using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.Sequences;

namespace Revo.DataAccess.EF6.Sequences
{
    public class TenantContextSequence<TTenantKey, TContextKey, TValue>: ITenantContextSequence<TTenantKey, TContextKey, TValue>
    {
        private readonly IDatabaseAccess databaseAccess;
        public TenantContextSequence(string name, TValue initialValue, TValue step, IDatabaseAccess databaseAccess)
        {
            Name = name;
            InitialValue = initialValue;
            Step = step;
            this.databaseAccess = databaseAccess;
        }
        
        public TValue InitialValue { get; private set; }
        public string Name { get; private set; }
        public TValue Step { get; private set; }

        public TValue NextValue(TTenantKey tenantKey, TContextKey contextKey)
        {
            var query = QueryNextValue(tenantKey, contextKey);
            return query.First();
        }

        public Task<TValue> NextValueAsync(TTenantKey tenantKey, TContextKey contextKey)
        {
            var query = QueryNextValue(tenantKey, contextKey);
            return query.FirstAsync();
        }

        private DbRawSqlQuery<TValue> QueryNextValue(TTenantKey tenantKey, TContextKey contextKey)
        {
            return databaseAccess.SqlQueryNontracked<TValue>(
                "EXEC [dbo].[GT_NEXT_TENANT_CONTEXT_SEQUENCE_VALUE] @p0, @p1, @p2, @p3, @p4",
                Name, tenantKey, contextKey, InitialValue, Step);
        }
    }
}
