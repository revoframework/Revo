using System.Linq;

namespace GTRevo.Commands
{
    public interface IEntityQuery<T> : IQuery<IQueryable<T>>
    {
    }
}
