using Ninject;
using Ninject.Modules;

namespace Revo.Core.Configuration
{
    public interface IKernelConfigurationContext
    {
        IKernel Kernel { get; }
        KernelConfigurationSection KernelConfigurationSection { get; }

        void LoadModule(INinjectModule module);
        void LoadModule<TModule>() where TModule : class, INinjectModule, new();
    }
}
