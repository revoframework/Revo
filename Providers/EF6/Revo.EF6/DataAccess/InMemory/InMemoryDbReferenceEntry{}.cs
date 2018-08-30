using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Revo.DataAccess.InMemory;
using Revo.EF6.DataAccess.Entities;

namespace Revo.EF6.DataAccess.InMemory
{
    public class InMemoryDbReferenceEntry<TEntity, TProperty> :
        InMemoryDbMemberEntry<TEntity, TProperty>,
        IDbReferenceEntry<TEntity, TProperty>
        where TEntity : class
    {
        public InMemoryDbReferenceEntry(Expression<Func<TEntity, TProperty>> navigationProperty,
            InMemoryCrudRepository.EntityEntry entityEntry) : base(navigationProperty, entityEntry)
        {
        }
        
        public bool IsLoaded { get; set; } = true;

        public void Load()
        {
            IsLoaded = true;
        }

        public Task LoadAsync()
        {
            return LoadAsync(default(CancellationToken));
        }

        public Task LoadAsync(CancellationToken cancellationToken)
        {
            Load();
            return Task.FromResult(0);
        }

        public IQueryable<TProperty> Query()
        {
            return new List<TProperty>() {CurrentValue}.AsQueryable();
        }
    }
}
