using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Security.ClaimBased;

namespace Revo.Core.Security
{
    [AutoLoadModule(false)]
    public class NullCoreSecurityModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IClaimsPrincipalUserResolver>()
                .To<NullClaimsPrincipalUserResolver>()
                .InSingletonScope();

            Bind<IUserPermissionResolver>()
                .To<NullUserPermissionResolver>()
                .InSingletonScope();
        }
    }
}
