using Hangfire.PostgreSql;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Revo.AspNetCore;
using Revo.AspNetCore.Configuration;
using Revo.Core.Configuration;
using Revo.EFCore.AspNetCoreOData.Configuration;
using Revo.EFCore.Configuration;
using Revo.EFCore.DataAccess.Configuration;
using Revo.EFCore.DataAccess.Conventions;
using Revo.Hangfire;

namespace Revo.Examples.Todos
{
    public class Startup : RevoStartup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddOData();
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            base.Configure(app, env, loggerFactory);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePages();
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        protected override IRevoConfiguration CreateRevoConfiguration()
        {
            /** TODO bugfix: for some reason, directly reading from Configuration in the closures in the UseXyz below
            causes occassional ArgumentNullExceptions (config is all null), using this as a workaround **/
            string connectionString = Configuration.GetConnectionString("TodosPostgreSQL");

            return new RevoConfiguration()
                .UseAspNetCore()
                .UseEFCoreDataAccess(
                    contextBuilder => contextBuilder
                        .UseNpgsql(connectionString),
                    advancedAction: cfg =>
                    {
                        cfg
                            .AddConvention<IdColumnsPrefixedWithTableNameConvention>(-110)
                            .AddConvention<PrefixConvention>(-9)
                            .AddConvention<SnakeCaseTableNamesConvention>(1)
                            .AddConvention<SnakeCaseColumnNamesConvention>(1)
                            .AddConvention<LowerCaseConvention>(2)
                            /*.AddConvention<ShortConstraintNamesEFCoreConvention>(2)*/;
                    })
                .UseAllEFCoreInfrastructure()
                .ConfigureEFCoreAspNetCoreOData(cfg => cfg.DisableAsyncQueryableResolution = true)
                .UseHangfire(new PostgreSqlStorage(connectionString));
        }
    }
}
