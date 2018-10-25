using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Infrastructure.Tenancy;

namespace Revo.Infrastructure.Globalization.Messages.Database
{
    public class DatabaseMessagesModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDbMessageCache>()
                .To<DbMessageCache>()
                .InTenantSingletonScope();

            Bind<IDbMessageLoader,
                    IEventListener<LocalizationMessageModifiedEvent>,
                    IEventListener<LocalizationMessageDeletedEvent>>()
                .To<DbMessageLoader>()
                .InTaskScope();

            Bind<IEventListener<DbMessageCacheReloadedEvent>>()
                .To<MessageRepositoryReloader>()
                .InTaskScope();
        }
    }
}
