using System.Linq;

namespace GTRevo.Platform.Commands
{
    public interface IEntityQueryableWrapper<T>
    {
        IQueryable<T> Result { get; set; }
    }
}
