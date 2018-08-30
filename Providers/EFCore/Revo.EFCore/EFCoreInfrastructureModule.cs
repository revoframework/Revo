using Ninject.Modules;
using Revo.EFCore.DataAccess.Conventions;
using Revo.EFCore.Domain;

namespace Revo.EFCore
{
    public class EFCoreInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEFCoreConvention>()
                .To<BasicDomainModelConvention>()
                .InSingletonScope();
        }
    }
}
