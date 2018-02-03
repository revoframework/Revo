using System.Linq;

namespace Revo.DataAccess.Entities
{
    public interface IRepositoryFilter
    {
        IQueryable<T> FilterResults<T>(IQueryable<T> results) where T : class;
        T FilterResult<T>(T result) where T : class;
        void FilterAdded<T>(T added) where T : class;
        void FilterDeleted<T>(T deleted) where T : class;
        void FilterModified<T>(T modified) where T : class;
    }
}
