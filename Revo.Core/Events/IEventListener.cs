using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Events
{
    public interface IEventListener<in T>
        where T : IEvent
    {
        Task HandleAsync(IEventMessage<T> message, CancellationToken cancellationToken);
    }
}
