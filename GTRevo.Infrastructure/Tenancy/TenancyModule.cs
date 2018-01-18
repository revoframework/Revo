using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.DataAccess.Entities;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Tenancy
{
    public class TenancyModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ITenantContext>()
                .To<DefaultTenantContext>()
                .InRequestOrJobScope();

            Bind<ITenantContextResolver>()
                .To<NullTenantContextResolver>()
                .InSingletonScope();

            Bind<IRepositoryFilter>()
                .To<TenantRepositoryFilter>()
                .InTransientScope();

            Bind<ITenantManager>()
                .To<DefaultTenantManager>()
                .InTransientScope();
        }
    }
}
