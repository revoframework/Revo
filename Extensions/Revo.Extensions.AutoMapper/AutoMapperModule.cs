using AutoMapper;
using Ninject.Extensions.ContextPreservation;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Extensions.AutoMapper
{
    [AutoLoadModule(false)]
    public class AutoMapperModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAutoMapperProfileDiscovery>()
                .To<AutoMapperProfileDiscovery>()
                .InSingletonScope();

            Bind<IAutoMapperInitializer, IApplicationConfigurer>()
                .To<AutoMapperInitializer>()
                .InSingletonScope();

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