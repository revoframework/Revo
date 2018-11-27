using Revo.DataAccess.InMemory;
using Revo.EFCore.DataAccess.Entities;

namespace Revo.EFCore.DataAccess.InMemory
{
    public class EFCoreInMemoryCrudRepository : InMemoryCrudRepository, IEFCoreCrudRepository
    {
        public IEFCoreDatabaseAccess DatabaseAccess { get; } = new EFCoreInMemoryDatabaseAccess();
    }
}
