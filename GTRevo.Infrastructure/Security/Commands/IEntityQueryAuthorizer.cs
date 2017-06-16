using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GTRevo.Commands;

namespace GTRevo.Infrastructure.Security.Commands
{
    public interface IEntityQueryAuthorizer
    {
        Task<IQueryable<T>> AuthorizeQueryAsync<T, TResult>(IQueryable<T> results,
            Expression<Func<T, TResult>> authorizedEntity, ICommandBase command);
    }
}