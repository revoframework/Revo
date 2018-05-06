using Ninject.Modules;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Security;
using Revo.Infrastructure.Security.Commands;

namespace Revo.Infrastructure.EF6.Security
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
