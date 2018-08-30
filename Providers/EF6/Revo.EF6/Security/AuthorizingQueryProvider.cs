using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Revo.Infrastructure.Security.Commands;

namespace Revo.EF6.Security
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
            var query = provider.CreateQuery(expression);
            query = InjectQueryable(query);
            return query;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var query = provider.CreateQuery<TElement>(expression);
            query = InjectQueryable<TElement>(query);
            return query;
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

        public IQueryable<T> InjectQueryable<T>(IQueryable<T> query)
        {
            DbQuery<T> dbQuery = query as DbQuery<T>;
            if (dbQuery != null)
            {
                var providerField = typeof(DbQuery<T>).GetField("_provider", BindingFlags.Instance | BindingFlags.NonPublic);
                providerField.SetValue(dbQuery, this);
            }

            return query;
        }

        public IQueryable InjectQueryable(IQueryable query)
        {
            var dbQueryType = typeof(DbQuery<>).MakeGenericType(query.ElementType);
            
            if (dbQueryType.IsInstanceOfType(query))
            {
                var providerField = dbQueryType.GetField("_provider", BindingFlags.Instance | BindingFlags.NonPublic);
                providerField.SetValue(query, this);
            }

            return query;
        }
    }
}
