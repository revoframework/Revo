using Ninject;
using Ninject.Modules;
using NSubstitute;
using Revo.Core.Configuration;
using Xunit;

namespace Revo.Core.Tests.Configuration
{
    public class KernelConfigurationContextTests
    {
        private KernelConfigurationContext sut;
        private readonly IKernel kernel;
        private readonly KernelConfigurationSection kernelConfigurationSection = new KernelConfigurationSection();

        public KernelConfigurationContextTests()
        {
            kernel = Substitute.For<IKernel>();
            sut = new KernelConfigurationContext(kernel, kernelConfigurationSection);
        }

        [Fact]
        public void LoadModule()
        {
            var module = Substitute.For<INinjectModule>();
            sut.LoadModule(module);
            kernel.Received(1).Load(Arg.Is<INinjectModule[]>(x => x.Length == 1 && x[0] == module));
        }

        [Fact]
        public void LoadModule_LoadsOnlyOnce()
        {
            var module = Substitute.For<INinjectModule>();
            module.Name.Returns("module");
            kernel.HasModule(module.Name).Returns(true);
            sut.LoadModule(module);
            kernel.DidNotReceiveWithAnyArgs().Load(module);
        }

        [Fact]
        public void LoadModule_DoesNotLoadIfDisabled()
        {
            var module = Substitute.For<INinjectModule>();
            kernelConfigurationSection.OverrideModuleLoading(module.GetType(), false);

            sut.LoadModule(module);
            kernel.DidNotReceiveWithAnyArgs().Load(module);
        }
    }
}
