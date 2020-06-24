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
using Revo.EFCore.Configuration;
using Revo.EFCore.DataAccess.Configuration;
using Revo.EFCore.DataAccess.Conventions;
using Revo.Extensions.AutoMapper.Configuration;

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

            services.AddControllers();
            services.AddRazorPages();
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
                endpoints.MapRazorPages();
            });
        }

        protected override IRevoConfiguration CreateRevoConfiguration()
        {
            return new RevoConfiguration()
                .UseAspNetCore()
                .UseEFCoreDataAccess(
                    contextBuilder => contextBuilder
                        .UseSqlite("Data Source=todos.db"), // NOTE: By default, this example uses SQLite database for simplicity. For real applications, you'll want to switch to more featured RDBMS as shown below.
                        // .UseNpgsql(connectionString) // for PostgreSQL
                        // .UseSqlServer(connectionString) // for SQL Server you will also need to comment out SnakeCaseColumnNamesConvention and LowerCaseConventionbelow
                    advancedAction: config =>
                    {
                        config
                            .AddConvention<BaseTypeAttributeConvention>(-200)
                            .AddConvention<IdColumnsPrefixedWithTableNameConvention>(-110)
                            .AddConvention<PrefixConvention>(-9)
                            .AddConvention<SnakeCaseTableNamesConvention>(1)
                            .AddConvention<SnakeCaseColumnNamesConvention>(1)
                            .AddConvention<LowerCaseConvention>(2);
                    })
                .UseAllEFCoreInfrastructure()
                .AddAutoMapperExtension();
        }
    }
}
