using System.Linq;

namespace GTRevo.Core.Commands
{
    public interface IEntityQuery<T> : IQuery<IQueryable<T>>
    {
    }
}
