using System.Data.Entity.ModelConfiguration.Conventions;
using Ninject.Modules;

namespace GTRevo.Infrastructure.EF6.DataAcccess
{
    public class InfrastructureDataAccessModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IConvention>()
               .To<CustomStoreConventionEx>()
               .InTransientScope();
        }
    }
}
