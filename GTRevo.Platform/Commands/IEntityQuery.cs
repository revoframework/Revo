using System.Linq;
using System.Web.OData.Query;

namespace GTRevo.Platform.Commands
{
    public interface IEntityQuery<T> : IQuery<IQueryable<T>>
    {
    }
}
