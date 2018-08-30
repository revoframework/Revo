using System.Threading.Tasks;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.DataAccess
{
    public interface IDatabaseInitializer
    {
        Task InitializeAsync(IRepository repository);
    }
}
