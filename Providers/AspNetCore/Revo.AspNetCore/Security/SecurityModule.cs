using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Security;

namespace Revo.AspNetCore.Security
{
    [AutoLoadModule(false)]
    public class SecurityModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IUserContext>()
                .To<AspNetCoreUserContext>()
                .InRequestOrJobScope();
        }
    }
}
