using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Security.Commands
{
    public interface IEntityQueryAuthorizer
    {
        Task<IQueryable<T>> AuthorizeQueryAsync<T, TResult>(IQueryable<T> results,
            Expression<Func<T, TResult>> authorizedEntity);

        Task<IQueryable<T>> AuthorizeQueryAsync<T, TResult>(IQueryable<T> results,
            Expression<Func<T, TResult>> authorizedEntity, ICommandBase command);
    }
}