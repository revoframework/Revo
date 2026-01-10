using System;
using Ninject;
using Ninject.Modules;

namespace Revo.Core.Configuration
{
    public class KernelConfigurationContext(IKernel kernel, KernelConfigurationSection kernelConfigurationSection) : IKernelConfigurationContext
    {
        public IKernel Kernel { get; } = kernel;
        public KernelConfigurationSection KernelConfigurationSection { get; } = kernelConfigurationSection;

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
