using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.InMemory;

namespace Revo.DataAccess.EF6.InMemory
{
    public class InMemoryDbEntityEntry<TEntity> : InMemoryDbEntityEntry, IDbEntityEntry<TEntity> where TEntity : class
    {
        public InMemoryDbEntityEntry(InMemoryCrudRepository.EntityEntry entityEntry) : base(entityEntry)
        {
        }

        public new TEntity Entity => (TEntity)base.Entity;

        public IDbCollectionEntry<TEntity, TElement> Collection<TElement>(Expression<Func<TEntity, ICollection<TElement>>> navigationProperty) where TElement : class
        {
            return new InMemoryDbCollectionEntry<TEntity, TElement>(navigationProperty, EntityEntry);
        }

        public IDbReferenceEntry<TEntity, TProperty> Reference<TProperty>(Expression<Func<TEntity, TProperty>> navigationProperty) where TProperty : class
        {
            return new InMemoryDbReferenceEntry<TEntity, TProperty>(navigationProperty, EntityEntry);
        }
    }
}
