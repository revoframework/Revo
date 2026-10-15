using System;
using Microsoft.Extensions.DependencyInjection;
using Revo.DataAccess.Entities;

namespace Revo.EFCore.DataAccess.Entities
{
    public class EFCoreCrudRepositoryFactory :
        ICrudRepositoryFactory<IReadRepository>,
        ICrudRepositoryFactory<ICrudRepository>,
        ICrudRepositoryFactory<IEFCoreReadRepository>,
        ICrudRepositoryFactory<IEFCoreCrudRepository>
    {
        private readonly IServiceProvider serviceProvider;

        public EFCoreCrudRepositoryFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IEFCoreCrudRepository Create()
        {
            var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory>();
            var requestDbContextCache = serviceProvider.GetRequiredService<IRequestDbContextCache>();
            var repositoryFilters = serviceProvider.GetServices<IRepositoryFilter>();
            var databaseAccess = new EFCoreDatabaseAccess(dbContextFactory, requestDbContextCache);
            return new EFCoreCrudRepository(repositoryFilters, databaseAccess);
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
