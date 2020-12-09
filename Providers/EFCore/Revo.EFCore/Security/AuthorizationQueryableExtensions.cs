using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.EFCore.DataAccess.Query;
using Revo.Infrastructure.Security.Commands;

namespace Revo.EFCore.Security
{
    public static class AuthorizationQueryableExtensions
    {
        public static Task<IQueryable<T>> AuthorizeAsync<T>(this IQueryable<T> queryable)
        {
            return queryable.AuthorizeAsync(x => x);
        }

        public static Task<IQueryable<T>> AuthorizeAsync<T>(this IQueryable<T> queryable,
            ICommandBase command)
        {
            return queryable.AuthorizeAsync(x => x, command);
        }

        public static async Task<IQueryable<T>> AuthorizeAsync<T, TResult>(this IQueryable<T> queryable,
            Expression<Func<T, TResult>> authorizedEntity)
        {
            var authorizer = GetEntityQueryAuthorizer(queryable);
            return await authorizer
                .AuthorizeQueryAsync(queryable, authorizedEntity);
        }

        public static async Task<IQueryable<T>> AuthorizeAsync<T, TResult>(this IQueryable<T> queryable,
            Expression<Func<T, TResult>> authorizedEntity, ICommandBase command)
        {
            var authorizer = GetEntityQueryAuthorizer(queryable);
            return await authorizer
                .AuthorizeQueryAsync(queryable, authorizedEntity, command);
        }

        private static IEntityQueryAuthorizer GetEntityQueryAuthorizer<T>(IQueryable<T> queryable)
        {
            var provider = queryable.Provider as CustomQueryProvider;
            if (provider == null)
            {
                throw new InvalidOperationException("Cannot authorize query - only queries from EF Core provider can be authorized. Please also make sure EnableCustomQueryProvider has been enabled during Revo provider configuration.");
            }

            return provider.ServiceLocator.Get<IEntityQueryAuthorizer>();
        }
    }
}