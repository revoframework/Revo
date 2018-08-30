using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Hangfire;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;
using Ninject.Web.Mvc;
using Ninject.Web.Mvc.Filter;
using Ninject.Web.WebApi;
using Revo.AspNet.Core.Lifecycle;
using Revo.Core.Configuration;
using Revo.Core.Core;

namespace Revo.AspNet.Core
{
    public abstract class RevoHttpApplication : NinjectHttpApplication
    {
        private readonly List<Assembly> loadedAssemblies = new List<Assembly>();
        private IRevoConfiguration revoConfiguration;
        private KernelBootstrapper kernelBootstrapper;

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
            kernelBootstrapper.LoadAssemblies(GetAllReferencedAssemblies());
            RegisterAspNetServices();
            kernelBootstrapper.RunAppConfigurers();
            kernelBootstrapper.RunAppStartListeners();
        }
        
        protected abstract IRevoConfiguration CreateRevoConfiguration();

        protected override void OnApplicationStarted()
        {
            System.Web.Http.GlobalConfiguration.Configure(httpConfiguration =>
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
            kernelBootstrapper.RunAppStopListeners();
        }

        protected override IKernel CreateKernel()
        {
            revoConfiguration = CreateRevoConfiguration();

            var kernel = new StandardKernel();
            loadedAssemblies.Clear();

            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => kernel);
                kernel.Bind<StandardKernel>().ToMethod(ctx => kernel as StandardKernel);

                GlobalConfiguration.Configuration.UseNinjectActivator(kernel);

                LocalConfiguration.Current = new WebConfiguration();
                NinjectBindingExtensions.Current = new AspNetNinjectBindingExtension();

                kernelBootstrapper = new KernelBootstrapper(kernel, revoConfiguration);
                kernelBootstrapper.Configure();

                var domainAssemblies = GetCurrentDomainAssemblies();
                kernelBootstrapper.LoadAssemblies(domainAssemblies);
                loadedAssemblies.AddRange(domainAssemblies);

                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        private Assembly[] GetCurrentDomainAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                   .Where(a => a.GetName().Name.StartsWith("System") == false)
                   .Where(a => a.GetName().Name.StartsWith("Ninject") == false)
                   .Where(a => !a.IsDynamic);

            return assemblies.ToArray();
        }

        private Assembly[] GetAllReferencedAssemblies()
        {
            var assemblies = System.Web.Compilation.BuildManager.GetReferencedAssemblies()
                   .Cast<Assembly>()
                   .Where(a => a.GetName().Name.StartsWith("System") == false)
                   .Where(a => a.GetName().Name.StartsWith("Ninject") == false)
                   .Where(a => !a.IsDynamic);

            return assemblies.Except(loadedAssemblies).ToArray();
        }

        private void RegisterAspNetServices()
        {
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

            DependencyResolver.SetResolver(kernel.Get<IDependencyResolver>());
            System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver = kernel.Get<Ninject.Web.WebApi.NinjectDependencyResolver>();
        }
    }
}
