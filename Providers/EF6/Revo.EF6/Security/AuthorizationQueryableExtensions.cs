using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Revo.Core.Commands;

namespace Revo.EF6.Security
{
    public static class AuthorizationQueryableExtensions
    {
        public static Task<IQueryable<T>> AuthorizeAsync<T>(this IQueryable<T> queryable,
            ICommandBase command)
        {
            return queryable.AuthorizeAsync(x => x, command);
        }

        public static async Task<IQueryable<T>> AuthorizeAsync<T, TResult>(this IQueryable<T> queryable,
            Expression<Func<T, TResult>> authorizedEntity, ICommandBase command)
        {
            var provider = queryable.Provider as AuthorizingQueryProvider;
            if (provider == null)
            {
                throw new InvalidOperationException("Only queries from IAuthorizingRepository can be authorized");
            }

            var entityQueryAuthorizer = provider.QueryAuthorizer;

            return await entityQueryAuthorizer
                .AuthorizeQueryAsync(queryable, authorizedEntity, command);
        }
    }
}
