using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Infrastructure.Tenancy;
using MediatR;
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
                    IAsyncNotificationHandler<LocalizationMessageModifiedEvent>,
                    IAsyncNotificationHandler<LocalizationMessageDeletedEvent>>()
                .To<DbMessageLoader>()
                .InRequestOrJobScope();
        }
    }
}
