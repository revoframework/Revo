using System.Linq;

namespace Revo.Core.Commands
{
    public interface IEntityQuery<T> : IQuery<IQueryable<T>>
    {
    }
}
