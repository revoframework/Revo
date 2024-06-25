using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.EF6.DataAccess.Entities
{
    public class WrapperDbEntityEntry<TEntity> : IDbEntityEntry<TEntity> where TEntity : class
    {
        private readonly DbEntityEntry<TEntity> inner;

        public WrapperDbEntityEntry(DbEntityEntry<TEntity> inner)
        {
            this.inner = inner;
        }

        public EntityState State
        {
            get => inner.State;
            set => inner.State = value;
        }

        public TEntity Entity => inner.Entity;

        public IDbCollectionEntry<TEntity, TElement> Collection<TElement>(Expression<Func<TEntity, ICollection<TElement>>> navigationProperty) where TElement : class
        {
            return new WrapperDbCollectionEntry<TEntity, TElement>(inner.Collection(navigationProperty));
        }

        public IDbReferenceEntry<TEntity, TProperty> Reference<TProperty>(Expression<Func<TEntity, TProperty>> navigationProperty) where TProperty : class
        {
            return new WrapperDbReferenceEntry<TEntity, TProperty>(inner.Reference(navigationProperty));
        }

        public void Reload() => inner.Reload();
        public Task ReloadAsync(CancellationToken cancellationToken) => inner.ReloadAsync(cancellationToken);
        public Task ReloadAsync() => inner.ReloadAsync();
    }
}
