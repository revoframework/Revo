using System;
using Revo.DataAccess.Entities;

namespace Revo.EFCore.DataAccess.Entities
{
    public class EFCoreCrudRepositoryFactory :
        ICrudRepositoryFactory<IReadRepository>,
        ICrudRepositoryFactory<ICrudRepository>,
        ICrudRepositoryFactory<IEFCoreReadRepository>,
        ICrudRepositoryFactory<IEFCoreCrudRepository>
    {
        private readonly Func<IDbContextFactory> dbContextFactoryFunc;
        private readonly Func<IRepositoryFilter[]> repositoryFiltersFunc;
        private readonly Func<IRequestDbContextCache> requestDbContextCacheFunc;

        public EFCoreCrudRepositoryFactory(Func<IDbContextFactory> dbContextFactoryFunc,
            Func<IRepositoryFilter[]> repositoryFiltersFunc,
            Func<IRequestDbContextCache> requestDbContextCacheFunc)
        {
            this.dbContextFactoryFunc = dbContextFactoryFunc;
            this.repositoryFiltersFunc = repositoryFiltersFunc;
            this.requestDbContextCacheFunc = requestDbContextCacheFunc;
        }

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
