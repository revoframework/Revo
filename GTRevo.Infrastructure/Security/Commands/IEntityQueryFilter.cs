using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GTRevo.Platform.Commands;

namespace GTRevo.Infrastructure.Security.Commands
{
    public interface IEntityQueryFilter
    {
    }

    public interface IEntityQueryFilter<T> : IEntityQueryFilter
    {
        Task<Expression<Func<T, bool>>> FilterAsync(ICommandBase query);
    }
}
