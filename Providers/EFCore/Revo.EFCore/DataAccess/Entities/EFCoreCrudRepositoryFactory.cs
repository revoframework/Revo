using System;
using Revo.DataAccess.Entities;

namespace Revo.EFCore.DataAccess.Entities
{
    public class EFCoreCrudRepositoryFactory(Func<IDbContextFactory> dbContextFactoryFunc,
            Func<IRepositoryFilter[]> repositoryFiltersFunc,
            Func<IRequestDbContextCache> requestDbContextCacheFunc) :
        ICrudRepositoryFactory<IReadRepository>,
        ICrudRepositoryFactory<ICrudRepository>,
        ICrudRepositoryFactory<IEFCoreReadRepository>,
        ICrudRepositoryFactory<IEFCoreCrudRepository>
    {
        public IEFCoreCrudRepository Create()
        {
            var databaseAccess = new EFCoreDatabaseAccess(dbContextFactoryFunc(), requestDbContextCacheFunc());
            return new EFCoreCrudRepository(repositoryFiltersFunc(), databaseAccess);
        }

        IReadRepository ICrudRepositoryFactory<IReadRepository>.Create()
        {
            return Create();
        }

        ICrudRepository ICrudRepositoryFactory<ICrudRepository>.Create()
        {
            return Create();
        }

        IEFCoreReadRepository ICrudRepositoryFactory<IEFCoreReadRepository>.Create()
        {
            return Create();
        }
    }
}
