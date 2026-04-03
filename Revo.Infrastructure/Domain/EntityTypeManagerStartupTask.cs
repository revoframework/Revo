using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Lifecycle;
using Revo.Domain.Entities;

namespace Revo.Infrastructure.Domain;

public sealed class EntityTypeManagerStartupTask(IEntityTypeManager entityTypeManager) : IStartupTask
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        entityTypeManager.ClearCache();
        return Task.CompletedTask;
    }
}
