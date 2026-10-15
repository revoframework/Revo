using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Events;

namespace Revo.Tools.DatabaseMigrator;

/// <summary>
/// Discards the events published by the migration provider, there are no listeners in the migrator tool.
/// </summary>
public class NullEventBus : IEventBus
{
    public Task PublishAsync(IEventMessage message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
