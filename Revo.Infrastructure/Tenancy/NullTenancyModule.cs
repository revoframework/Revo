using System;
using System.Collections.Generic;
using System.Text;
using Ninject.Modules;

namespace Revo.Infrastructure.Tenancy
{
    public class NullTenancyModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ITenantContextResolver>()
                .To<NullTenantContextResolver>()
                .InSingletonScope();
        }
    }
}
