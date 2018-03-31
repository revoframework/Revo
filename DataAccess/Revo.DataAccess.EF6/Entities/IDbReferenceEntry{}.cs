using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.DataAccess.EF6.Entities
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
