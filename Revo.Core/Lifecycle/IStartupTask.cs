using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Lifecycle;

/// <summary>
/// Work executed once at host startup, resolved from a DI scope (see <see cref="ScopedHostedService{T}"/>).
/// </summary>
public interface IStartupTask
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
