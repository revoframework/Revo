using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Security.Commands
{
    public interface IEntityQueryFilter
    {
    }

    public interface IEntityQueryFilter<in TBase> : IEntityQueryFilter
    {
        Task<Expression<Func<T, bool>>> FilterAsync<T>(ICommandBase query) where T : TBase;
    }
}
