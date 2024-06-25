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
    public class DatabaseInitializerLoader(IDatabaseInitializerDiscovery databaseInitializerDiscovery,
            IDatabaseInitializerSorter sorter, Func<IRepository> repositoryFunc,
            Func<IUnitOfWorkFactory> unitOfWorkFactoryFunc, Func<CommandContextStack> commandContextStackFunc)
        : IDatabaseInitializerLoader, IApplicationStartedListener
    {
        private bool isInitialized = false;

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
