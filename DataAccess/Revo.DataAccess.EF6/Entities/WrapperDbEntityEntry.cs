using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.DataAccess.EF6.Entities
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
