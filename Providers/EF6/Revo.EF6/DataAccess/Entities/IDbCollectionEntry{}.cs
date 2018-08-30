using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.EF6.DataAccess.Entities
{
    public interface IDbCollectionEntry<TEntity, TElement> : IDbMemberEntry<TEntity, ICollection<TElement>>
        where TEntity : class
        where TElement : class
    {
        bool IsLoaded { get; set; }
        void Load();
        Task LoadAsync();
        Task LoadAsync(CancellationToken cancellationToken);
        IQueryable<TElement> Query();
    }
}
