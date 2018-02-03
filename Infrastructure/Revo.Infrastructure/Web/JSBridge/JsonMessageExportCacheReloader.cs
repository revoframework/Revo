using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Infrastructure.Globalization;

namespace Revo.Infrastructure.Web.JSBridge
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
