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

        public EFCoreCrudRepositoryFactory(Func<IDbContextFactory> dbContextFactoryFunc,
            Func<IRepositoryFilter[]> repositoryFiltersFunc)
        {
            this.dbContextFactoryFunc = dbContextFactoryFunc;
            this.repositoryFiltersFunc = repositoryFiltersFunc;
        }

        public IEFCoreCrudRepository Create()
        {
            return new EFCoreCrudRepository(dbContextFactoryFunc(), repositoryFiltersFunc());
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
