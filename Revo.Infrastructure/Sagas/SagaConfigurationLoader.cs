using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.Sagas;

public class SagaConfigurationLoader(
    IEnumerable<ISagaConfigurator> sagaConfigurators,
    ISagaRegistry sagaRegistry) : IStartupTask
{
    private readonly ISagaConfigurator[] sagaConfigurators = sagaConfigurators.ToArray();

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (ISagaConfigurator sagaConfigurator in sagaConfigurators)
        {
            sagaConfigurator.ConfigureSagas(sagaRegistry);
        }

        return Task.CompletedTask;
    }
}
