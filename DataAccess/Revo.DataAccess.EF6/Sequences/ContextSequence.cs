using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.Sequences;

namespace Revo.DataAccess.EF6.Sequences
{
    public class ContextSequence<TContextKey, TValue> : IContextSequence<TContextKey, TValue>
    {
        private readonly IDatabaseAccess databaseAccess;

        public ContextSequence(string name, TValue initialValue, TValue step,
            IDatabaseAccess databaseAccess)
        {
            Name = name;
            InitialValue = initialValue;
            Step = step;

            this.databaseAccess = databaseAccess;
        }

        public TValue InitialValue { get; private set; }
        public string Name { get; private set; }
        public TValue Step { get; private set; }

        public TValue NextValue(TContextKey contextKey)
        {
            var query = QueryNextValue(contextKey);
            return query.First();
        }

        public Task<TValue> NextValueAsync(TContextKey contextKey)
        {
            var query = QueryNextValue(contextKey);
            return query.FirstAsync();
        }

        private DbRawSqlQuery<TValue> QueryNextValue(TContextKey contextKey)
        {
            return databaseAccess.SqlQueryNontracked<TValue>(
                "EXEC [dbo].[GT_NEXT_CONTEXT_SEQUENCE_VALUE] @p0, @p1, @p2, @p3",
                Name, contextKey, InitialValue, Step);
        }
    }
}
