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
    public class InMemoryDbEntityEntry : IDbEntityEntry
    {
        public InMemoryDbEntityEntry(InMemoryCrudRepository.EntityEntry entityEntry)
        {
            this.EntityEntry = entityEntry;
        }

        public EntityState State
        {
            get
            {
                switch (EntityEntry.State)
                {
                    case DataAccess.Entities.EntityState.Detached:
                        return EntityState.Detached;
                    case DataAccess.Entities.EntityState.Unchanged:
                        return EntityState.Unchanged;
                    case DataAccess.Entities.EntityState.Added:
                        return EntityState.Added;
                    case DataAccess.Entities.EntityState.Deleted:
                        return EntityState.Deleted;
                    case DataAccess.Entities.EntityState.Modified:
                        return EntityState.Modified;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            set
            {
                switch (value)
                {
                    case EntityState.Detached:
                        EntityEntry.State = DataAccess.Entities.EntityState.Detached;
                        break;
                    case EntityState.Unchanged:
                        EntityEntry.State = DataAccess.Entities.EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        EntityEntry.State = DataAccess.Entities.EntityState.Added;
                        break;
                    case EntityState.Deleted:
                        EntityEntry.State = DataAccess.Entities.EntityState.Deleted;
                        break;
                    case EntityState.Modified:
                        EntityEntry.State = DataAccess.Entities.EntityState.Modified;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        public object Entity => EntityEntry.Instance;
        
        protected InMemoryCrudRepository.EntityEntry EntityEntry { get; }

        public Task ReloadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task ReloadAsync()
        {
            return Task.FromResult(0);
        }

        public void Reload()
        {
        }

        Task IDbEntityEntry.ReloadAsync(CancellationToken cancellationToken)
        {
            return ReloadAsync(cancellationToken);
        }

        Task IDbEntityEntry.ReloadAsync()
        {
            return ReloadAsync();
        }

        void IDbEntityEntry.Reload()
        {
            Reload();
        }
    }
}
