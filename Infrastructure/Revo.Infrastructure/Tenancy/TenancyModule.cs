using Ninject.Modules;
using Revo.Core.Core;
using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.Tenancy
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
                .WhenNoAncestorMatches(ctx => typeof(ITenantProvider).IsAssignableFrom(ctx.Request.Service))
                .InTransientScope();

            Bind<ITenantProvider>()
                .To<DefaultTenantProvider>()
                .InTransientScope();
        }
    }
}
