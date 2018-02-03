using Hangfire;
using Hangfire.MemoryStorage;
using Owin;
using Revo.Platforms.AspNet.Core.Lifecycle;

namespace Revo.Platforms.AspNet.Core
{
    public class HangfireOwinConfigurator : IOwinConfigurator
    {
        public void ConfigureApp(IAppBuilder app)
        {
#if DEBUG
            GlobalConfiguration.Configuration.UseStorage<MemoryStorage>(new MemoryStorage());
#else
            GlobalConfiguration.Configuration.UseSqlServerStorage("EntityContext");
#endif

            app.UseHangfireDashboard();

            /*var options = new BackgroundJobServerOptions { WorkerCount = 1 };

            CrawlerServiceProcess crawlerServiceProcess =
                (CrawlerServiceProcess)System.Web.Mvc.DependencyResolver.Current.GetService(typeof(CrawlerServiceProcess));*/
            app.UseHangfireServer(/*options, crawlerServiceProcess*/);
        }
    }
}
