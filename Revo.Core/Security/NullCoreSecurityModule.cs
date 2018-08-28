using System;
using System.Collections.Generic;
using System.Text;
using Ninject.Modules;

namespace Revo.Core.Security
{
    public class NullCoreSecurityModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IUserPermissionResolver>()
                .To<NullUserPermissionResolver>()
                .InSingletonScope();

            Bind<IRolePermissionResolver>()
                .To<NullRolePermissionResolver>()
                .InSingletonScope();
        }

    }
}
