using Revo.AspNet;
using Revo.AspNet.Core;
using Revo.Core.Configuration;
using Revo.EF6;
using Revo.EF6.DataAccess;
using Revo.Hangfire;

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
