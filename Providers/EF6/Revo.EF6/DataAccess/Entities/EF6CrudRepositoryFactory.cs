using System;
using Revo.DataAccess.Entities;
using Revo.EF6.DataAccess.Model;

namespace Revo.EF6.DataAccess.Entities
{
    public class EF6CrudRepositoryFactory :
        ICrudRepositoryFactory<IReadRepository>,
        ICrudRepositoryFactory<ICrudRepository>,
        ICrudRepositoryFactory<IEF6ReadRepository>,
        ICrudRepositoryFactory<IEF6CrudRepository>
    {
        private readonly Func<IModelMetadataExplorer> modelMetadataExplorerFunc;
        private readonly Func<IDbContextFactory> dbContextFactoryFunc;
        private readonly Func<IRepositoryFilter[]> repositoryFiltersFunc;

        public EF6CrudRepositoryFactory(Func<IModelMetadataExplorer> modelMetadataExplorerFunc,
            Func<IDbContextFactory> dbContextFactoryFunc, Func<IRepositoryFilter[]> repositoryFiltersFunc)
        {
            this.modelMetadataExplorerFunc = modelMetadataExplorerFunc;
            this.dbContextFactoryFunc = dbContextFactoryFunc;
            this.repositoryFiltersFunc = repositoryFiltersFunc;
        }

        public IEF6CrudRepository Create()
        {
            return new EF6CrudRepository(modelMetadataExplorerFunc(), dbContextFactoryFunc(), repositoryFiltersFunc());
        }

        IReadRepository ICrudRepositoryFactory<IReadRepository>.Create()
        {
            return Create();
        }

        ICrudRepository ICrudRepositoryFactory<ICrudRepository>.Create()
        {
            return Create();
        }

        IEF6ReadRepository ICrudRepositoryFactory<IEF6ReadRepository>.Create()
        {
            return Create();
        }
    }
}
