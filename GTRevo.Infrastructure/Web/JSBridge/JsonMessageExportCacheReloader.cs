using System;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Globalization;

namespace GTRevo.Infrastructure.Web.JSBridge
{
    public class DbMessageCacheReloader :
        IEventListener<MessageRepositoryReloadedEvent>
    {
        private readonly JsonMessageExportCache jsonMessageExportCache;

        public DbMessageCacheReloader(JsonMessageExportCache jsonMessageExportCache)
        {
            this.jsonMessageExportCache = jsonMessageExportCache;
        }

        public Task HandleAsync(IEventMessage<MessageRepositoryReloadedEvent> message, CancellationToken cancellationToken)
        {
            // TODO run this in a background job
            jsonMessageExportCache.Refresh();
            return Task.FromResult(0);
        }
    }
}
