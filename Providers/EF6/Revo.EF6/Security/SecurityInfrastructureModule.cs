using Ninject.Modules;
using Revo.DataAccess.Entities;

namespace Revo.EF6.Security
{
    public class EF6SecurityInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepositoryFilter>()
                .To<AuthorizingCrudRepositoryFilter>()
                .InTransientScope();
        }
    }
}
