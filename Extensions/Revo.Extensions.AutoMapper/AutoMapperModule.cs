using AutoMapper;
using Ninject.Extensions.ContextPreservation;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Extensions.AutoMapper.Configuration;

namespace Revo.Extensions.AutoMapper
{
    [AutoLoadModule(false)]
    public class AutoMapperModule(AutoMapperConfigurationSection section) : NinjectModule
    {
        public override void Load()
        {
            Bind<IAutoMapperProfileDiscovery>()
                .To<AutoMapperProfileDiscovery>()
                .InSingletonScope();

            Bind<IAutoMapperInitializer, IApplicationConfigurer>()
                .To<AutoMapperInitializer>()
                .InSingletonScope();
            
            Bind<AutoMapperConfigurationSection>()
                .ToConstant(section);

            Bind<MapperConfiguration>()
                .ToMethod(ctx => ctx.ContextPreservingGet<IAutoMapperInitializer>()
                    .GetMapperConfiguration())
                .InSingletonScope();

            Bind<IMapper>()
                .ToMethod(ctx => ctx.ContextPreservingGet<IAutoMapperInitializer>()
                    .CreateMapper())
                .InTaskScope();
        }
    }
}