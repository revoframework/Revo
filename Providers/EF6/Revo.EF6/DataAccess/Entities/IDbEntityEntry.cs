using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.EF6.DataAccess.Entities
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
