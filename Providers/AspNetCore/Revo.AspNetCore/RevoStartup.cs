using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.ContextPreservation;
using Ninject.Extensions.Factory;
using Ninject.Infrastructure.Disposal;
using Revo.AspNetCore.Ninject;
using Revo.Core.Configuration;
using Revo.Core.Core;
using Revo.Core.Types;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Revo.AspNetCore
{
    public abstract class RevoStartup
    {
        private static readonly AsyncLocal<Scope> scopeProvider = new AsyncLocal<Scope>();
        public static object RequestScope(IContext context) => scopeProvider.Value;
        
        private readonly List<Assembly> loadedAssemblies = new List<Assembly>();

        public RevoStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IKernel Kernel { get; private set; }
        private object Resolve(Type type) => Kernel.Get(type);

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddRequestScopingMiddleware(() => scopeProvider.Value = new Scope());
            services.AddCustomControllerActivation(Resolve);
            services.AddCustomViewComponentActivation(Resolve);
        }
        
        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Kernel = CreateKernel(app, loggerFactory);

            var revoConfiguration = CreateRevoConfiguration();
            var kernelBootstrapper = new KernelBootstrapper(Kernel, revoConfiguration);

            var typeExplorer = new TypeExplorer();

            kernelBootstrapper.Configure();
            kernelBootstrapper.LoadAssemblies(typeExplorer.GetAllReferencedAssemblies());
            
            kernelBootstrapper.RunAppConfigurers();
            kernelBootstrapper.RunAppStartListeners();
        }

        protected abstract IRevoConfiguration CreateRevoConfiguration();

        private IKernel CreateKernel(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            Kernel = new StandardKernel();

            Kernel.Load(new FuncModule());
            Kernel.Load(new ContextPreservationModule());

            Kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => Kernel);
            Kernel.Bind<StandardKernel>().ToMethod(ctx => Kernel as StandardKernel);

            //Hangfire.GlobalConfiguration.Configuration.UseNLogLogProvider();
            //Hangfire.GlobalConfiguration.Configuration.UseNinjectActivator();

            NinjectBindingExtensions.Current = new AspNetCoreNinjectBindingExtension();

            RegisterAspNetCoreService(app, loggerFactory);

            return Kernel;
        }

        private void RegisterAspNetCoreService(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            foreach (var ctrlType in app.GetControllerTypes())
            {
                Kernel.Bind(ctrlType).ToSelf().InScope(RequestScope);
            }

            Kernel.Bind<IViewBufferScope>().ToMethod(ctx => app.GetRequestService<IViewBufferScope>());
            Kernel.Bind<ILoggerFactory>().ToConstant(loggerFactory);
            Kernel.Bind<IHttpContextAccessor>()
                .ToMethod(ctx => app.ApplicationServices.GetRequiredService<IHttpContextAccessor>())
                .InTransientScope();
            Kernel.Bind<HttpContext>()
                .ToMethod(ctx => app.ApplicationServices.GetRequiredService<IHttpContextAccessor>().HttpContext)
                .InTransientScope();

            Kernel.Bind<IConfiguration>().ToConstant(Configuration);
        }
        
        private sealed class Scope : DisposableObject { }
    }
}
