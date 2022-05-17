using System;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Core.Transactions;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.DataAccess
{
    public class DatabaseInitializerLoader : IDatabaseInitializerLoader, IApplicationStartedListener
    {
        private readonly IDatabaseInitializerDiscovery databaseInitializerDiscovery;
        private readonly IDatabaseInitializerSorter sorter;
        private readonly Func<IRepository> repositoryFunc; // using func factories for late resolving in the scope of different tasks
        private readonly Func<IUnitOfWorkFactory> unitOfWorkFactoryFunc;
        private readonly Func<CommandContextStack> commandContextStackFunc;
        private bool isInitialized = false;

        public DatabaseInitializerLoader(IDatabaseInitializerDiscovery databaseInitializerDiscovery,
            IDatabaseInitializerSorter sorter, Func<IRepository> repositoryFunc,
            Func<IUnitOfWorkFactory> unitOfWorkFactoryFunc, Func<CommandContextStack> commandContextStackFunc)
        {
            this.databaseInitializerDiscovery = databaseInitializerDiscovery;
            this.sorter = sorter;
            this.repositoryFunc = repositoryFunc;
            this.unitOfWorkFactoryFunc = unitOfWorkFactoryFunc;
            this.commandContextStackFunc = commandContextStackFunc;
        }

        public void OnApplicationStarted()
        {
            EnsureDatabaseInitialized();
        }

        public void EnsureDatabaseInitialized()
        {
            if (!isInitialized)
            {
                isInitialized = true;

                var initializers = databaseInitializerDiscovery.DiscoverDatabaseInitializers();
                var sortedInitializers = sorter.GetSorted(initializers.ToArray());

                foreach (var initializer in sortedInitializers)
                {
                    Task.Factory.StartNewWithContext(async () =>
                    {
                        using (IUnitOfWork uow = unitOfWorkFactoryFunc().CreateUnitOfWork())
                        {
                            var commandContextStack = commandContextStackFunc();
                            commandContextStack.Push(new CommandContext(null, uow));
                            try
                            {
                                uow.Begin();
                                await initializer.InitializeAsync(repositoryFunc());
                                await uow.CommitAsync();
                            }
                            finally
                            {
                                commandContextStack.Pop();
                            }
                        }
                    }).GetAwaiter().GetResult();
                }
            }
        }
    }
}
