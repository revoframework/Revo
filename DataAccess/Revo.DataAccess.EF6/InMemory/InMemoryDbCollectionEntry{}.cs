using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.InMemory;

namespace Revo.DataAccess.EF6.InMemory
{
    public class InMemoryDbCollectionEntry<TEntity, TElement> :
        InMemoryDbMemberEntry<TEntity, ICollection<TElement>>,
        IDbCollectionEntry<TEntity, TElement>
        where TEntity : class
        where TElement : class
    {
        public InMemoryDbCollectionEntry(Expression<Func<TEntity, ICollection<TElement>>> navigationProperty,
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

        public IQueryable<TElement> Query()
        {
            return CurrentValue.AsQueryable();
        }
    }
}
