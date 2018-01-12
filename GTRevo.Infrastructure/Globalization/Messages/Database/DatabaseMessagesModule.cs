using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Tenancy;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Globalization.Messages.Database
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
                .InRequestOrJobScope();

            Bind<IEventListener<DbMessageCacheReloadedEvent>>()
                .To<MessageRepositoryReloader>()
                .InRequestOrJobScope();
        }
    }
}
