using Revo.Core.Configuration;
using Revo.DataAccess.EF6;
using Revo.Infrastructure.EF6;
using Revo.Infrastructure.Hangfire;
using Revo.Platforms.AspNet;
using Revo.Platforms.AspNet.Core;

namespace Revo.Examples.HelloAspNet.Bootstrap
{
    public class MvcApplication : RevoHttpApplication
    {
        protected override IRevoConfiguration CreateRevoConfiguration()
        {
            return new RevoConfiguration()
                .UseAspNet()
                .UseEF6DataAccess(useAsPrimaryRepository: true, connection: new EF6ConnectionConfiguration("EntityContext"))
                .UseAllEF6Infrastructure()
                .UseHangfire();
        }
    }
}
