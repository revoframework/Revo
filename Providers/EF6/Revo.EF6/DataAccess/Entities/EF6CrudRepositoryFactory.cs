using System;
using Revo.DataAccess.Entities;
using Revo.EF6.DataAccess.Model;

namespace Revo.EF6.DataAccess.Entities
{
    public class EF6CrudRepositoryFactory(Func<IModelMetadataExplorer> modelMetadataExplorerFunc,
            Func<IDbContextFactory> dbContextFactoryFunc, Func<IRepositoryFilter[]> repositoryFiltersFunc,
            Func<IRequestDbContextCache> requestDbContextCacheFunc) :
        ICrudRepositoryFactory<IReadRepository>,
        ICrudRepositoryFactory<ICrudRepository>,
        ICrudRepositoryFactory<IEF6ReadRepository>,
        ICrudRepositoryFactory<IEF6CrudRepository>
    {
        public IEF6CrudRepository Create()
        {
            var databaseAccess = new EF6DatabaseAccess(dbContextFactoryFunc(),
                requestDbContextCacheFunc(), modelMetadataExplorerFunc());
            return new EF6CrudRepository(databaseAccess, repositoryFiltersFunc());
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
