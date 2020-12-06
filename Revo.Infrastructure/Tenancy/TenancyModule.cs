using Ninject.Modules;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.Tenancy
{
    public class TenancyModule : NinjectModule
    {
        private readonly TenancyConfiguration configuration;

        public TenancyModule(TenancyConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public override void Load()
        {
            Bind<ITenantContext>()
                .To<DefaultTenantContext>()
                .InTaskScope();

            if (configuration.EnableTenantRepositoryFilter)
            {
                Bind<IRepositoryFilter>()
                    .To<TenantRepositoryFilter>()
                    .InTransientScope()
                    .WithPropertyValue(nameof(TenantRepositoryFilter.NullTenantCanAccessOtherTenantsData), configuration.NullTenantCanAccessOtherTenantsData);
            }

            if (configuration.UseNullTenantContextResolver)
            {
                Bind<ITenantContextResolver>()
                    .To<NullTenantContextResolver>()
                    .InSingletonScope();
            }

            if (configuration.UseDefaultTenantProvider)
            {
                Bind<ITenantProvider>()
                    .To<DefaultTenantProvider>()
                    .InTransientScope();
            }
            
            Bind<ICommandBusMiddleware<ICommandBase>>()
                .To<TenantContextCommandBusMiddleware>()
                .InTransientScope();
        }
    }
}
