using System;
using System.Collections.Generic;
using Ninject.Modules;

namespace Revo.Core.Configuration
{
    public class KernelConfigurationSection : IRevoConfigurationSection
    {
        private readonly Dictionary<Type, bool> loadedModuleOverrides = new Dictionary<Type, bool>();
        private readonly List<Action<IKernelConfigurationContext>> kernelActions = new List<Action<IKernelConfigurationContext>>();

        public KernelConfigurationSection()
        {
        }

        public IReadOnlyDictionary<Type, bool> LoadedModuleOverrides => loadedModuleOverrides;
        public IReadOnlyCollection<Action<IKernelConfigurationContext>> KernelActions => kernelActions;

        public void AddAction(Action<IKernelConfigurationContext> kernelAction)
        {
            kernelActions.Add(kernelAction);
        }

        public void OverrideModuleLoading(Type moduleType, bool isLoaded)
        {
            if (!typeof(INinjectModule).IsAssignableFrom(moduleType))
            {
                throw new ArgumentException($"Invalid module type to OverrideModuleLoading: {moduleType}");
            }

            loadedModuleOverrides[moduleType] = isLoaded;
        }
    }
}
