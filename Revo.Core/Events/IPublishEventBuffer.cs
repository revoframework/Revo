using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Events
{
    public interface IPublishEventBuffer
    {
        IReadOnlyCollection<IEventMessage> Events { get; }
        Task FlushAsync(CancellationToken cancellationToken);
        void PushEvent(IEventMessage message);
    }
}