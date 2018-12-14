using Revo.DataAccess.Entities;
using Revo.EFCore.UnitOfWork;

namespace Revo.EFCore.DataAccess.Entities
{
    public interface IEFCoreCrudRepository : ICrudRepository, IEFCoreReadRepository
    {
    }
}
