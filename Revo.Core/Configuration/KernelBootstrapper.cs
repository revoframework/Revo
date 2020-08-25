using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ninject;
using Ninject.Modules;
using NLog;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Core.Configuration
{
    public class KernelBootstrapper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IKernel kernel;
        private readonly IRevoConfiguration configuration;
        private readonly HashSet<Assembly> loadedAssemblies = new HashSet<Assembly>();

        public KernelBootstrapper(IKernel kernel, IRevoConfiguration configuration)
        {
            this.kernel = kernel;
            this.configuration = configuration;
        }

        public void Configure()
        {
            var kernelSection = configuration.GetSection<KernelConfigurationSection>();
            var kernelConfigurationContext = new KernelConfigurationContext(kernel, kernelSection);
            var kernelActions = kernelSection.KernelActions;
            foreach (var action in kernelActions)
            {
                action(kernelConfigurationContext);
            }
        }

        public void LoadAssemblies(IReadOnlyCollection<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                if (loadedAssemblies.Contains(assembly))
                {
                    continue;
                }
                
                var modules = GetNinjectModules(assembly).Where(x => !kernel.HasModule(x.Name)).ToArray();
                if (modules.Length > 0)
                {
                    Logger.Info($"Loading {modules.Length} dependency modules from assembly {assembly.FullName}: {string.Join(",", modules.Select(x => x.Name))}");
                    kernel.Load(modules);
                }

                loadedAssemblies.Add(assembly);
            }
        }

        public void RunAppConfigurers()
        {
            var initializer = kernel.Get<IApplicationConfigurerInitializer>();
            initializer.ConfigureAll();
        }

        public void RunAppStartListeners()
        {
            var initializer = kernel.Get<IApplicationStartListenerInitializer>();
            initializer.InitializeStarted();
        }

        public void RunAppStopListeners()
        {
            var initializer = kernel.Get<IApplicationStartListenerInitializer>();
            initializer.DeinitializeStopping();
        }

        private INinjectModule[] GetNinjectModules(Assembly assembly)
        {
            return assembly.IsDynamic
                ? new INinjectModule[0]
                : assembly.ExportedTypes.Where(IsLoadableModule)
                    .Select(type => Activator.CreateInstance(type) as INinjectModule)
                    .ToArray();
        }

        private bool IsModuleEnabled(Type type)
        {
            var autoLoadAttr = type.GetCustomAttribute<AutoLoadModuleAttribute>();
            bool autoLoad = autoLoadAttr?.AutoLoad ?? true;

            if (configuration.GetSection<KernelConfigurationSection>().LoadedModuleOverrides
                .TryGetValue(type, out bool autoLoadOverride))
            {
                autoLoad = autoLoadOverride;
            }

            return autoLoad;
        }

        private bool IsLoadableModule(Type type)
        {
            if (!typeof(INinjectModule).IsAssignableFrom(type)
                || type.IsAbstract
                || type.IsInterface
                || type.GetConstructor(Type.EmptyTypes) == null)
            {
                return false;
            }

            return IsModuleEnabled(type);
        }
    }
}
