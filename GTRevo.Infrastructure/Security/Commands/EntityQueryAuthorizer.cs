using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GTRevo.Core.Commands;
using LinqKit;

namespace GTRevo.Infrastructure.Security.Commands
{
    public class EntityQueryAuthorizer : IEntityQueryAuthorizer
    {
        private readonly IEntityQueryFilterFactory entityQueryFilterFactory;

        public EntityQueryAuthorizer(IEntityQueryFilterFactory entityQueryFilterFactory)
        {
            this.entityQueryFilterFactory = entityQueryFilterFactory;
        }

        public async Task<IQueryable<T>> AuthorizeQueryAsync<T, TResult>(IQueryable<T> results,
            Expression<Func<T, TResult>> authorizedEntity, ICommandBase command)
        {
            var authorized = results;

            foreach (var filter in entityQueryFilterFactory.GetEntityQueryFilters<TResult>())
            {
                var filterExpression = await filter.FilterAsync(command);

                var tParam = Expression.Parameter(typeof(T), "__x");
                var tResult = Expression.Invoke(authorizedEntity, tParam);
                var whereBody = Expression.Invoke(filterExpression, tResult);
                var whereExpression = Expression.Lambda<Func<T, bool>>(whereBody, tParam);
                whereExpression = whereExpression.Expand();

                authorized = authorized.Where(whereExpression);
            }

            return authorized;
        }
    }
}
