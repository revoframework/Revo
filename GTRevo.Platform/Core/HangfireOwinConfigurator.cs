using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Platform.Core.Lifecycle;
using Hangfire;
using Owin;

namespace GTRevo.Platform.Core
{
    public class HangfireOwinConfigurator : IOwinConfigurator
    {
        public void ConfigureApp(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("EntityContext");
            //Hangfire.GlobalConfiguration.Configuration.UseStorage<MemoryStorage>(new MemoryStorage());

            app.UseHangfireDashboard();

            /*var options = new BackgroundJobServerOptions { WorkerCount = 1 };

            CrawlerServiceProcess crawlerServiceProcess =
                (CrawlerServiceProcess)System.Web.Mvc.DependencyResolver.Current.GetService(typeof(CrawlerServiceProcess));*/
            app.UseHangfireServer(/*options, crawlerServiceProcess*/);
        }
    }
}
