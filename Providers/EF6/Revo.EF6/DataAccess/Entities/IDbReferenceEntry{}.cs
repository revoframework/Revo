using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.EF6.DataAccess.Entities
{
    public interface IDbReferenceEntry<TEntity, TProperty> : IDbMemberEntry<TEntity, TProperty>
        where TEntity : class
    {
        bool IsLoaded { get; set; }
        void Load();
        Task LoadAsync();
        Task LoadAsync(CancellationToken cancellationToken);
        IQueryable<TProperty> Query();
    }
}
