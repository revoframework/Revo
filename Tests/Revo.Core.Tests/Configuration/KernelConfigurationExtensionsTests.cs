using System;
using System.Collections.Generic;
using FluentAssertions;
using Ninject.Modules;
using NSubstitute;
using Revo.Core.Configuration;
using Xunit;

namespace Revo.Core.Tests.Configuration
{
    public class KernelConfigurationExtensionsTests
    {
        private KernelConfigurationSection kernelConfigurationSection = new KernelConfigurationSection();
        private IRevoConfiguration revoConfiguration;

        public KernelConfigurationExtensionsTests()
        {
            revoConfiguration = Substitute.For<IRevoConfiguration>();
            revoConfiguration.GetSection<KernelConfigurationSection>().Returns(kernelConfigurationSection);
        } 

        [Fact]
        public void ConfigureKernel()
        {
            var action = Substitute.For<Action<IKernelConfigurationContext>>();
            revoConfiguration.ConfigureKernel(action);

            kernelConfigurationSection.KernelActions.Should().Contain(action);
        }

        [Fact]
        public void ConfigureKernel_Advanced()
        {
            bool configured = false;
            revoConfiguration.ConfigureKernel(ctx => { },
                kcs =>
                {
                    kcs.Should().Be(kernelConfigurationSection);
                    configured = true;
                });

            configured.Should().BeTrue();
        }


        [Fact]
        public void OverrideModuleLoading()
        {
            var action = Substitute.For<Action<IKernelConfigurationContext>>();
            revoConfiguration.OverrideModuleLoading(typeof(NinjectModule), false);

            kernelConfigurationSection.LoadedModuleOverrides.Should()
                .Contain(new KeyValuePair<Type, bool>(typeof(NinjectModule), false));
        }
    }
}
