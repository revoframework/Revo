using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using Revo.DataAccess.InMemory;
using Revo.EF6.DataAccess.Entities;

namespace Revo.EF6.DataAccess.InMemory
{
    public class InMemoryDbEntityEntry : IDbEntityEntry
    {
        public InMemoryDbEntityEntry(InMemoryCrudRepository.EntityEntry entityEntry)
        {
            this.EntityEntry = entityEntry;
        }

        public System.Data.Entity.EntityState State
        {
            get
            {
                switch (EntityEntry.State)
                {
                    case Revo.DataAccess.Entities.EntityState.Detached:
                        return EntityState.Detached;
                    case Revo.DataAccess.Entities.EntityState.Unchanged:
                        return EntityState.Unchanged;
                    case Revo.DataAccess.Entities.EntityState.Added:
                        return EntityState.Added;
                    case Revo.DataAccess.Entities.EntityState.Deleted:
                        return EntityState.Deleted;
                    case Revo.DataAccess.Entities.EntityState.Modified:
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
                        EntityEntry.State = Revo.DataAccess.Entities.EntityState.Detached;
                        break;
                    case EntityState.Unchanged:
                        EntityEntry.State = Revo.DataAccess.Entities.EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        EntityEntry.State = Revo.DataAccess.Entities.EntityState.Added;
                        break;
                    case EntityState.Deleted:
                        EntityEntry.State = Revo.DataAccess.Entities.EntityState.Deleted;
                        break;
                    case EntityState.Modified:
                        EntityEntry.State = Revo.DataAccess.Entities.EntityState.Modified;
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
