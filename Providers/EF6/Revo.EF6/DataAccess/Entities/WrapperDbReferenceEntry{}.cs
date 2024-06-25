﻿using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.EF6.DataAccess.Entities
{
    public class WrapperDbReferenceEntry<TEntity, TProperty>(DbReferenceEntry<TEntity, TProperty> inner) : IDbReferenceEntry<TEntity, TProperty>
        where TEntity : class
        where TProperty : class
    {
        public string Name => inner.Name;

        public TProperty CurrentValue
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
        public IQueryable<TProperty> Query() => inner.Query();
    }
}
