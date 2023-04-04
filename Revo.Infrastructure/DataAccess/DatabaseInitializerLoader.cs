using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Revo.Core.Commands;
using Revo.Core.Transactions;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.DataAccess;

public class DatabaseInitializerLoader(
    IDatabaseInitializerDiscovery databaseInitializerDiscovery,
    IDatabaseInitializerSorter sorter,
    IServiceScopeFactory serviceScopeFactory) : IDatabaseInitializerLoader, IHostedLifecycleService
{
    private readonly SemaphoreSlim initSemaphore = new(1, 1);
    private bool isInitialized;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        await EnsureDatabaseInitializedAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void EnsureDatabaseInitialized()
    {
        EnsureDatabaseInitializedAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task EnsureDatabaseInitializedAsync(CancellationToken cancellationToken = default)
    {
        await initSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;

            var initializers = databaseInitializerDiscovery.DiscoverDatabaseInitializers();
            var sortedInitializers = sorter.GetSorted(initializers.ToArray());

            foreach (var initializer in sortedInitializers)
            {
                using var scope = serviceScopeFactory.CreateScope();
                var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
                var commandContextStack = scope.ServiceProvider.GetRequiredService<CommandContextStack>();
                var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

                using IUnitOfWork uow = unitOfWorkFactory.CreateUnitOfWork();
                commandContextStack.Push(new CommandContext(null, uow));
                try
                {
                    uow.Begin();
                    await initializer.InitializeAsync(repository).ConfigureAwait(false);
                    await uow.CommitAsync().ConfigureAwait(false);
                }
                finally
                {
                    commandContextStack.Pop();
                }
            }
        }
        finally
        {
            initSemaphore.Release();
        }
    }
}
