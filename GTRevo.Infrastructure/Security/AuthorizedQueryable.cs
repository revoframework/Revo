using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Platform.Commands;

namespace GTRevo.Infrastructure.Security
{
    public class AuthorizedQueryable<T> : IQueryable<T>, IDbAsyncEnumerable<T>
    {
        private readonly AuthorizingQueryProvider provider;

        public AuthorizedQueryable(AuthorizingQueryProvider provider,
            Expression expression)
        {
            this.provider = provider;
            Expression = expression;
        }

        public Type ElementType => typeof(T);
        public Expression Expression { get; }
        public IQueryProvider Provider => provider;

        public IEnumerator<T> GetEnumerator()
        {
            return provider.ExecuteQuery<T>(Expression);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return provider.ExecuteQuery<T>(Expression);
        }

        public IDbAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return provider.ExecuteQueryAsync<T>(Expression);
        }

        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }
    }
}
