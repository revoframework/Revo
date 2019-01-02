using Revo.DataAccess.Entities;

namespace Revo.EFCore.DataAccess.Entities
{
    public interface IEFCoreCrudRepository : ICrudRepository, IEFCoreReadRepository
    {
    }
}
