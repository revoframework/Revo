using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.DataAccess.EF6.Entities
{
    public interface IDbEntityEntry<TEntity>
        where TEntity : class
    {
        EntityState State { get; set; }
        TEntity Entity { get; }

        IDbCollectionEntry<TEntity, TElement> Collection<TElement>(Expression<Func<TEntity, ICollection<TElement>>> navigationProperty) where TElement : class;
        IDbReferenceEntry<TEntity, TProperty> Reference<TProperty>(Expression<Func<TEntity, TProperty>> navigationProperty) where TProperty : class;

        void Reload();
        Task ReloadAsync(CancellationToken cancellationToken);
        Task ReloadAsync();
    }
}
