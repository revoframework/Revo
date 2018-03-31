using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.DataAccess.EF6.Entities
{
    public interface IDbEntityEntry
    {
        EntityState State { get; set; }
        object Entity { get; }

        void Reload();
        Task ReloadAsync(CancellationToken cancellationToken);
        Task ReloadAsync();
    }
}
