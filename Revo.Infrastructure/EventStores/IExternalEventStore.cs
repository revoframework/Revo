using System;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Infrastructure.Events.Async.Generic;

namespace Revo.Infrastructure.Events
{
    public interface IExternalEventStore
    {
        Task<ExternalEventRecord> GetEventAsync(Guid eventId);
        Task<ExternalEventRecord[]> CommitAsync();
        void PushEvent(IEventMessage eventMessage);
    }
}
