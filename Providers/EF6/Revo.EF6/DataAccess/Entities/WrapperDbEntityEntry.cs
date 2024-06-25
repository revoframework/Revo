using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.EF6.DataAccess.Entities
{
    public class WrapperDbEntityEntry : IDbEntityEntry
    {
        private readonly DbEntityEntry inner;

        public WrapperDbEntityEntry(DbEntityEntry inner)
        {
            this.inner = inner;
        }

        public EntityState State
        {
            get => inner.State;
            set => inner.State = value;
        }

        public object Entity => inner.Entity;
        public void Reload() => inner.Reload();
        public Task ReloadAsync(CancellationToken cancellationToken) => inner.ReloadAsync(cancellationToken);
        public Task ReloadAsync() => inner.ReloadAsync();
    }
}
