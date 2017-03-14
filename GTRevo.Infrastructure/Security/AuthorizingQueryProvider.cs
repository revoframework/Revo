using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Security.Commands;

namespace GTRevo.Infrastructure.Security
{
    public class AuthorizingQueryProvider : IDbAsyncQueryProvider
    {
        private readonly IQueryProvider provider;
        private readonly IDbAsyncQueryProvider asyncProvider;

        public AuthorizingQueryProvider(IQueryProvider provider, IEntityQueryAuthorizer entityQueryAuthorizer)
        {
            this.provider = provider;
            asyncProvider = provider as IDbAsyncQueryProvider;
            
            QueryAuthorizer = entityQueryAuthorizer;
        }
        
        public IEntityQueryAuthorizer QueryAuthorizer { get; }

        public IEnumerator<TElement> ExecuteQuery<TElement>(
            Expression expression)
        {
            return provider.CreateQuery<TElement>(expression).GetEnumerator();
        }
        public IDbAsyncEnumerator<TElement> ExecuteQueryAsync<TElement>(
            Expression expression)
        {
            var query = asyncProvider.CreateQuery<TElement>(expression) as IDbAsyncEnumerable<TElement>;

            if (query == null)
            {
                throw new InvalidOperationException(
                    "Decorated query provider of AuthorizingQueryProvider doesn't support async queries: " +
                    provider.ToString());
            }

            return query.GetAsyncEnumerator();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return provider.CreateQuery(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new AuthorizedQueryable<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            return provider.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return provider.Execute<TResult>(expression);
        }

        public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            if (asyncProvider == null)
            {
                throw new InvalidOperationException(
                    "Decorated query provider of AuthorizingQueryProvider doesn't support async queries: " +
                    provider.ToString());
            }

            return asyncProvider.ExecuteAsync(expression, cancellationToken);
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            if (asyncProvider == null)
            {
                throw new InvalidOperationException(
                    "Decorated query provider of AuthorizingQueryProvider doesn't support async queries: " +
                    provider.ToString());
            }

            return asyncProvider.ExecuteAsync<TResult>(expression, cancellationToken);
        }
    }
}
