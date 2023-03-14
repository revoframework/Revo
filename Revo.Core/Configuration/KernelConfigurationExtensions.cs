using System;
using Ninject.Modules;

namespace Revo.Core.Configuration
{
    public static class KernelConfigurationExtensions
    {
        public static IRevoConfiguration ConfigureKernel(this IRevoConfiguration configuration,
            Action<IKernelConfigurationContext> configureAction,
            Action<KernelConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<KernelConfigurationSection>();

            section.AddAction(configureAction);

            advancedAction?.Invoke(section);

            return configuration;
        }

        public static IRevoConfiguration OverrideModuleLoading(this IRevoConfiguration configuration,
            Type moduleType, bool isLoaded,
            Action<KernelConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<KernelConfigurationSection>();
            section.OverrideModuleLoading(moduleType, isLoaded);
            advancedAction?.Invoke(section);

            return configuration;
        }

        public static IRevoConfiguration OverrideModuleLoading<TModule>(this IRevoConfiguration configuration,
            bool isLoaded,
            Action<KernelConfigurationSection> advancedAction = null) where TModule : INinjectModule
        {
            return OverrideModuleLoading(configuration, typeof(TModule), isLoaded, advancedAction);
        }
    }
}
