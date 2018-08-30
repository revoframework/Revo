using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Revo.DataAccess.InMemory;
using Revo.EF6.DataAccess.Entities;

namespace Revo.EF6.DataAccess.InMemory
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
