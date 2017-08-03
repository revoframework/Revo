using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using GTRevo.Core;
using GTRevo.Core.Core;
using Hangfire;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Mvc;
using Ninject.Web.Mvc.Filter;
using Ninject.Web.Mvc.Validation;
using Ninject.Web.WebApi;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

namespace GTRevo.Platform.Core
{
    public static class NinjectWebLoader
    {
        private static readonly List<Assembly> LoadedAssemblies = new List<Assembly>();

        public static Bootstrapper Bootstrapper { get; private set; } = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);
        }

        public static void PostStart()
        {
            RegisterPostServices(Bootstrapper.Kernel);
            
            var kernel = (StandardKernel)Bootstrapper.Kernel;
            kernel.Settings.AllowNullInjection = true;
            kernel.Components.Add<INinjectHttpApplicationPlugin, NinjectMvcHttpApplicationPlugin>();
            //kernel.Bind<IDependencyResolver>().To<NinjectDependencyResolver>();
            kernel.Bind<IFilterProvider>().To<NinjectFilterAttributeFilterProvider>();
            kernel.Bind<IFilterProvider>().To<NinjectFilterProvider>();
            kernel.Bind<ModelValidatorProvider>().To<NinjectDataAnnotationsModelValidatorProvider>();

            kernel.Components.Add<INinjectHttpApplicationPlugin, NinjectWebApiHttpApplicationPlugin>();
            kernel.Components.Add<IWebApiRequestScopeProvider, DefaultWebApiRequestScopeProvider>();

            kernel.Bind<System.Web.Http.Dependencies.IDependencyResolver>().To<Ninject.Web.WebApi.NinjectDependencyResolver>();

            /*kernel.Bind<IFilterProvider>().To<DefaultFilterProvider>();
            kernel.Bind<IFilterProvider>().To<NinjectFilterProvider>();

            kernel.Bind<ModelValidatorProvider>().To<NinjectDefaultModelValidatorProvider>();
            kernel.Bind<ModelValidatorProvider>().To<NinjectDataAnnotationsModelValidatorProvider>();*/

            DependencyResolver.SetResolver(((StandardKernel)Bootstrapper.Kernel).Get<IDependencyResolver>());
            GlobalConfiguration.Configuration.DependencyResolver = ((StandardKernel)Bootstrapper.Kernel).Get<Ninject.Web.WebApi.NinjectDependencyResolver>();
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }

        public static IEnumerable<T> ResolveAll<T>()
        {
            if (Bootstrapper.Kernel == null)
            {
                throw new InvalidOperationException("Bootstrapper.Kernel has not been initialized yet");
            }

            var kernel = (StandardKernel)Bootstrapper.Kernel;
            return kernel.GetAll<T>();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            LoadedAssemblies.Clear();

            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<StandardKernel>().ToMethod(ctx => Bootstrapper.Kernel as StandardKernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

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

        private static void RegisterCoreServices(IKernel kernel)
        {
            //kernel.BindMediatR(); //NOTE: loaded automatically

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                   .Where(a => a.GetName().Name.StartsWith("System") == false)
                   .Where(a => a.GetName().Name.StartsWith("Ninject") == false)
                   .Where(a => a.GetName().Name.StartsWith("MediatR") == false)                   
                   .Where(a => !a.IsDynamic)
                   .ToList();

            assemblies = assemblies.Except(LoadedAssemblies).ToList();
            LoadedAssemblies.AddRange(assemblies);

            kernel.Load(assemblies);
        }

        private static void RegisterPostServices(IKernel kernel)
        {
            var assemblies = System.Web.Compilation.BuildManager.GetReferencedAssemblies()
                   .Cast<Assembly>()
                   .Where(a => a.GetName().Name.StartsWith("System") == false)
                   .Where(a => a.GetName().Name.StartsWith("Ninject") == false)
                   .Where(a => a.GetName().Name.StartsWith("MediatR") == false)                   
                   .Where(a => !a.IsDynamic)
                   .ToList();

            assemblies = assemblies.Except(LoadedAssemblies).ToList();
            LoadedAssemblies.AddRange(assemblies);

            kernel.Load(assemblies);
        }
    }
}
