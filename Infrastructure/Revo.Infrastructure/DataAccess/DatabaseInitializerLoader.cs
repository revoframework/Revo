using System;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Core.Lifecycle;
using Revo.Core.Transactions;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.DataAccess
{
    public class DatabaseInitializerLoader : IApplicationStartListener
    {
        private readonly IDatabaseInitializerDiscovery databaseInitializerDiscovery;
        private readonly IDatabaseInitializerComparer comparer;
        private readonly Func<IRepositoryFactory> repositoryFactoryFunc; // using func factories for late resolving in the scope of different tasks
        private readonly Func<IUnitOfWorkFactory> unitOfWorkFactoryFunc;

        public DatabaseInitializerLoader(IDatabaseInitializerDiscovery databaseInitializerDiscovery,
            IDatabaseInitializerComparer comparer, Func<IRepositoryFactory> repositoryFactoryFunc,
            Func<IUnitOfWorkFactory> unitOfWorkFactoryFunc)
        {
            this.databaseInitializerDiscovery = databaseInitializerDiscovery;
            this.comparer = comparer;
            this.repositoryFactoryFunc = repositoryFactoryFunc;
            this.unitOfWorkFactoryFunc = unitOfWorkFactoryFunc;
        }

        public void OnApplicationStarted()
        {
            var initializers = databaseInitializerDiscovery.DiscoverDatabaseInitializers();
            var sortedInitializers = initializers.ToList();
            sortedInitializers.Sort(comparer);

            foreach (var initializer in sortedInitializers)
            {
                Task.Factory.StartNewWithContext(async () =>
                {
                    using (IUnitOfWork uow = unitOfWorkFactoryFunc().CreateUnitOfWork())
                    using (IRepository repository = repositoryFactoryFunc().CreateRepository(uow))
                    {
                        await initializer.InitializeAsync(repository);
                        await uow.CommitAsync();
                    }
                });
            }
        }
    }
}
