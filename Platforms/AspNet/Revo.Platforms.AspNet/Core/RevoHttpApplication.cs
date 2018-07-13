using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Hangfire;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;
using Ninject.Web.Mvc;
using Ninject.Web.Mvc.Filter;
using Ninject.Web.WebApi;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Platforms.AspNet.Core.Lifecycle;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;
using NinjectFilterProvider = Ninject.Web.Mvc.Filter.NinjectFilterProvider;

namespace Revo.Platforms.AspNet.Core
{
    public class RevoHttpApplication : NinjectHttpApplication
    {
        private readonly List<Assembly> loadedAssemblies = new List<Assembly>();

        public RevoHttpApplication()
        {
        }

        public static RevoHttpApplication Current => (RevoHttpApplication) HttpContext.Current.ApplicationInstance;

        public T Resolve<T>()
        {
            if (Kernel == null)
            {
                throw new InvalidOperationException("Kernel has not been initialized yet");
            }

            var kernel = (StandardKernel)Kernel;
            return kernel.Get<T>();
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            if (Kernel == null)
            {
                throw new InvalidOperationException("Kernel has not been initialized yet");
            }

            var kernel = (StandardKernel)Kernel;
            return kernel.GetAll<T>();
        }

        public void PostStart()
        {
            RegisterPostServices(Kernel);

            var kernel = (StandardKernel)Kernel;
            kernel.Settings.AllowNullInjection = true;
            
            kernel.Components.Add<INinjectHttpApplicationPlugin, NinjectMvcHttpApplicationPlugin>();
            //kernel.Bind<IDependencyResolver>().To<NinjectDependencyResolver>();
            kernel.Bind<IFilterProvider>().To<NinjectFilterAttributeFilterProvider>();
            kernel.Bind<IFilterProvider>().To<NinjectFilterProvider>();
            //kernel.Bind<System.Web.Http.Validation.ModelValidatorProvider>().To<NinjectDataAnnotationsModelValidatorProvider>();

            kernel.Components.Add<INinjectHttpApplicationPlugin, NinjectWebApiHttpApplicationPlugin>();
            //kernel.Components.Add<IWebApiRequestScopeProvider, DefaultWebApiRequestScopeProvider>();

            //kernel.Bind<System.Web.Http.Dependencies.IDependencyResolver>().To<Ninject.Web.WebApi.NinjectDependencyResolver>();

           // kernel.Bind<System.Web.Http.Filters.IFilterProvider>().To<DefaultFilterProvider>();
            kernel.Bind<IFilterProvider>().To<NinjectFilterProvider>();

            DependencyResolver.SetResolver(Kernel.Get<IDependencyResolver>());
            GlobalConfiguration.Configuration.DependencyResolver = Kernel.Get<Ninject.Web.WebApi.NinjectDependencyResolver>();

            var configurer = kernel.Get<IApplicationConfigurerInitializer>();
            configurer.ConfigureAll();
        }

        protected override void OnApplicationStarted()
        {
            GlobalConfiguration.Configure(httpConfiguration =>
            {
                WebApiConfig.Register(httpConfiguration);

                foreach (IHttpApplicationInitializer appInitializer in ResolveAll<IHttpApplicationInitializer>())
                {
                    appInitializer.OnApplicationStart(this);
                }
            });
        }

        protected override void OnApplicationStopped()
        {
            base.OnApplicationStopped();
        }
        
        protected override IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            loadedAssemblies.Clear();

            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => Kernel);
                kernel.Bind<StandardKernel>().ToMethod(ctx => Kernel as StandardKernel);
                
                Hangfire.GlobalConfiguration.Configuration.UseNinjectActivator(kernel);

                NinjectBindingExtensions.Current = new AspNetNinjectBindingExtension();

                RegisterCoreServices(kernel);

                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        private void RegisterCoreServices(IKernel kernel)
        {
            LocalConfiguration.Current = new WebConfiguration();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                   .Where(a => a.GetName().Name.StartsWith("System") == false)
                   .Where(a => a.GetName().Name.StartsWith("Ninject") == false)
                   .Where(a => !a.IsDynamic)
                   .ToList();

            assemblies = assemblies.Except(loadedAssemblies).ToList();
            loadedAssemblies.AddRange(assemblies);

            kernel.Load(assemblies);
        }

        private void RegisterPostServices(IKernel kernel)
        {
            var assemblies = System.Web.Compilation.BuildManager.GetReferencedAssemblies()
                   .Cast<Assembly>()
                   .Where(a => a.GetName().Name.StartsWith("System") == false)
                   .Where(a => a.GetName().Name.StartsWith("Ninject") == false)
                   .Where(a => !a.IsDynamic)
                   .ToList();

            assemblies = assemblies.Except(loadedAssemblies).ToList();
            loadedAssemblies.AddRange(assemblies);

            kernel.Load(assemblies);
        }
    }
}
