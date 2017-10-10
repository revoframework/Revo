using System;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Globalization.Messages.Database;
using MediatR;

namespace GTRevo.Infrastructure.Web.JSBridge
{
    public class DbMessageCacheReloader :
        IAsyncNotificationHandler<DbMessageCacheReloadedEvent>
    {
        private readonly JsonMessageExportCache jsonMessageExportCache;

        public DbMessageCacheReloader(JsonMessageExportCache jsonMessageExportCache)
        {
            this.jsonMessageExportCache = jsonMessageExportCache;
        }

        public Task Handle(DbMessageCacheReloadedEvent notification)
        {
            // TODO run this in a background job
            jsonMessageExportCache.Refresh();
            return Task.FromResult(0);
        }
    }
}
