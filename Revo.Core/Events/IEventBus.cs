using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Events
{
    public interface IEventBus
    {
        Task PublishAsync(IEventMessage message, CancellationToken cancellationToken = default(CancellationToken));
    }
}
