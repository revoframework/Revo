using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Core.Transactions;

namespace GTRevo.Core.Events
{
    public interface IPublishEventBuffer
    {
        IReadOnlyCollection<IEventMessage> Events { get; }
        Task FlushAsync(CancellationToken cancellationToken);
        void PushEvent(IEventMessage message);
    }
}