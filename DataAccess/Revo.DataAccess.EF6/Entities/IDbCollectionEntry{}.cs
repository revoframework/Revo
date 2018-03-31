using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.DataAccess.EF6.Entities
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
