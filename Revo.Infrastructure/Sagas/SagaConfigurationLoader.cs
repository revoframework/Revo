using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Revo.Infrastructure.Sagas;

public class SagaConfigurationLoader(
    IEnumerable<ISagaConfigurator> sagaConfigurators,
    ISagaRegistry sagaRegistry) : IHostedLifecycleService
{
    private readonly ISagaConfigurator[] sagaConfigurators = sagaConfigurators.ToArray();

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        foreach (ISagaConfigurator sagaConfigurator in sagaConfigurators)
        {
            sagaConfigurator.ConfigureSagas(sagaRegistry);
        }

        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
