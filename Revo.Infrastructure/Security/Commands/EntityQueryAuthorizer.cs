using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LinqKit;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Security.Commands
{
    public class EntityQueryAuthorizer : IEntityQueryAuthorizer
    {
        private readonly IEntityQueryFilterFactory entityQueryFilterFactory;
        private readonly ICommandContext commandContext;

        public EntityQueryAuthorizer(IEntityQueryFilterFactory entityQueryFilterFactory, ICommandContext commandContext)
        {
            this.entityQueryFilterFactory = entityQueryFilterFactory;
            this.commandContext = commandContext;
        }

        public Task<IQueryable<T>> AuthorizeQueryAsync<T, TResult>(IQueryable<T> results, Expression<Func<T, TResult>> authorizedEntity)
        {
            var command = commandContext.CurrentCommand;
            return AuthorizeQueryAsync(results, authorizedEntity, command);
        }

        public async Task<IQueryable<T>> AuthorizeQueryAsync<T, TResult>(IQueryable<T> results,
            Expression<Func<T, TResult>> authorizedEntity, ICommandBase command)
        {
            var authorized = results;

            foreach (var filter in entityQueryFilterFactory.GetEntityQueryFilters<TResult>())
            {
                var filterExpression = await filter.FilterAsync<TResult>(command);

                if (filterExpression == null)
                { 
                    continue;
                }

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
