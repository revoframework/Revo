﻿using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.EF6.DataAccess.Entities
{
    public class WrapperDbCollectionEntry<TEntity, TElement>(DbCollectionEntry<TEntity, TElement> inner) : IDbCollectionEntry<TEntity, TElement>
        where TEntity : class
        where TElement : class
    {
        public string Name => inner.Name;

        public ICollection<TElement> CurrentValue
        {
            get => inner.CurrentValue;
            set => inner.CurrentValue = value;

        }

        public bool IsLoaded
        {
            get => inner.IsLoaded;
            set => inner.IsLoaded = value;
        }

        public void Load() => inner.Load();
        public Task LoadAsync() => inner.LoadAsync();
        public Task LoadAsync(CancellationToken cancellationToken) => inner.LoadAsync(cancellationToken);
        public IQueryable<TElement> Query() => inner.Query();
    }
}
