using System;
using Ninject;
using Ninject.Modules;

namespace Revo.Core.Configuration
{
    public class KernelConfigurationContext : IKernelConfigurationContext
    {
        public KernelConfigurationContext(IKernel kernel, KernelConfigurationSection kernelConfigurationSection)
        {
            Kernel = kernel;
            KernelConfigurationSection = kernelConfigurationSection;
        }

        public IKernel Kernel { get; }
        public KernelConfigurationSection KernelConfigurationSection { get; }

        public void LoadModule<TModule>() where TModule : class, INinjectModule, new()
        {
            var module = new TModule();
            if (!Kernel.HasModule(module.Name) && !IsModuleDisabled(typeof(TModule)))
            {
                Kernel.Load(module);
            }
        }

        public void LoadModule(INinjectModule module)
        {
            if (!Kernel.HasModule(module.Name) && !IsModuleDisabled(module.GetType()))
            {
                Kernel.Load(module);
            }
        }

        private bool IsModuleDisabled(Type moduleType)
        {
            return KernelConfigurationSection.LoadedModuleOverrides
                       .TryGetValue(moduleType, out bool isEnabled)
                   && !isEnabled;
        }
    }
}
