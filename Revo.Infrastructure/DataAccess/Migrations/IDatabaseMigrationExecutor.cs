using System.Threading.Tasks;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface IDatabaseMigrationExecutor
    {
        Task ExecuteAsync();
    }
}