using System;
using System.Collections.Generic;
using FluentAssertions;
using Ninject.Modules;
using NSubstitute;
using Revo.Core.Configuration;
using Xunit;

namespace Revo.Core.Tests.Configuration
{
    public class KernelConfigurationSectionTests
    {
        private KernelConfigurationSection sut;

        public KernelConfigurationSectionTests()
        {
            sut = new KernelConfigurationSection();
        }

        [Fact]
        public void AddAction()
        {
            var action = Substitute.For<Action<IKernelConfigurationContext>>();
            sut.AddAction(action);

            sut.KernelActions.Should().Contain(action);
        }

        [Fact]
        public void OverrideModuleLoading()
        {
            sut.OverrideModuleLoading(typeof(NinjectModule), false);
            sut.LoadedModuleOverrides.Should().Contain(new KeyValuePair<Type, bool>(typeof(NinjectModule), false));
        }
    }
}
