using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using NSubstitute;
using Revo.Core.Configuration;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Xunit;

namespace Revo.Core.Tests.Configuration
{
    public class KernelBootstrapperTests
    {
        private KernelBootstrapper sut;
        private readonly IKernel kernel;
        private readonly IRevoConfiguration revoConfiguration;
        private readonly KernelConfigurationSection kernelConfigurationSection = new KernelConfigurationSection();
        private readonly List<INinjectModule> loadedModules = new List<INinjectModule>();

        public KernelBootstrapperTests()
        {
            kernel = Substitute.For<IKernel>();
            kernel.WhenForAnyArgs(x => x.Load((IEnumerable<INinjectModule>)null)).Do(ci => loadedModules.AddRange(ci.ArgAt<IEnumerable<INinjectModule>>(0)));
            kernel.CreateRequest(null, null, null, false, false).ReturnsForAnyArgs(ci =>
            {
                var request = Substitute.For<IRequest>();
                request.Service.Returns(ci.ArgAt<Type>(0));
                return request;
            });

            revoConfiguration = Substitute.For<IRevoConfiguration>();
            revoConfiguration.GetSection<KernelConfigurationSection>().Returns(kernelConfigurationSection);

            sut = new KernelBootstrapper(kernel, revoConfiguration, new NullLogger<KernelBootstrapper>());
        }

        [Fact]
        public void Configure_RunsAllActionsInOrder()
        {
            kernelConfigurationSection.AddAction(Substitute.For<Action<IKernelConfigurationContext>>());
            kernelConfigurationSection.AddAction(Substitute.For<Action<IKernelConfigurationContext>>());

            sut.Configure();

            Received.InOrder(() =>
            {
                kernelConfigurationSection.KernelActions.ElementAt(0)
                    .Invoke(Arg.Is<IKernelConfigurationContext>(x =>
                        x.KernelConfigurationSection == kernelConfigurationSection && x.Kernel == kernel));
                kernelConfigurationSection.KernelActions.ElementAt(1)
                    .Invoke(Arg.Is<IKernelConfigurationContext>(x =>
                        x.KernelConfigurationSection == kernelConfigurationSection && x.Kernel == kernel));
            });
        }

        [Fact]
        public void LoadAssemblies()
        {
            sut.LoadAssemblies(new [] { this.GetType().Assembly });

            loadedModules.Should().HaveCount(2);
            loadedModules.Should().Contain(x => x.GetType() == typeof(TestModule));
            loadedModules.Should().Contain(x => x.GetType() == typeof(TestModuleDefaultEnabled));
        }

        [Fact]
        public void LoadAssemblies_DisablingOverride()
        {
            kernelConfigurationSection.OverrideModuleLoading(typeof(TestModule), false);
            kernelConfigurationSection.OverrideModuleLoading(typeof(TestModuleDefaultEnabled), false);
            sut.LoadAssemblies(new [] { this.GetType().Assembly });

            loadedModules.Should().BeEmpty();
        }

        [Fact]
        public void LoadAssemblies_EnablingOverride()
        {
            kernelConfigurationSection.OverrideModuleLoading(typeof(TestModuleDefaultDisabled), true);
            sut.LoadAssemblies(new[] { this.GetType().Assembly });

            loadedModules.Should().HaveCount(3);
            loadedModules.Should().Contain(x => x.GetType() == typeof(TestModule));
            loadedModules.Should().Contain(x => x.GetType() == typeof(TestModuleDefaultEnabled));
            loadedModules.Should().Contain(x => x.GetType() == typeof(TestModuleDefaultDisabled));
        }

        [Fact]
        public void LoadAssemblies_LoadsOnlyOnce()
        {
            kernel.HasModule(typeof(TestModule).FullName).Returns(true);

            sut.LoadAssemblies(new[] { this.GetType().Assembly });

            loadedModules.Should().HaveCount(1);
            loadedModules.Should().Contain(x => x.GetType() == typeof(TestModuleDefaultEnabled));
        }

        [Fact]
        public void RunAppConfigurers()
        {
            var initializer = Substitute.For<IApplicationConfigurerInitializer>();
            kernel.Resolve(Arg.Is<IRequest>(x => x.Service == typeof(IApplicationConfigurerInitializer))).Returns(new[] { initializer });
            sut.RunAppConfigurers();
            initializer.Received(1).ConfigureAll();
        }

        [Fact]
        public void RunAppStartListeners()
        {
            var initializer = Substitute.For<IApplicationLifecycleNotifier>();
            kernel.Resolve(Arg.Is<IRequest>(x => x.Service == typeof(IApplicationLifecycleNotifier))).Returns(new[] { initializer });
            sut.RunAppStartListeners();

            Received.InOrder(() =>
            {
                initializer.NotifyStarting();
                initializer.NotifyStarted();
            });
        }

        [Fact]
        public void RunAppStopListeners()
        {
            var initializer = Substitute.For<IApplicationLifecycleNotifier>();
            kernel.Resolve(Arg.Is<IRequest>(x => x.Service == typeof(IApplicationLifecycleNotifier))).Returns(new[] { initializer });
            sut.RunAppStopListeners();
            initializer.Received(1).NotifyStopping();
        }

        public class TestModule : NinjectModule
        {
            public override void Load()
            {
            }
        }
        
        [AutoLoadModule(true)]
        public class TestModuleDefaultEnabled : NinjectModule
        {
            public override void Load()
            {
            }
        }

        [AutoLoadModule(false)]
        public class TestModuleDefaultDisabled : NinjectModule
        {
            public override void Load()
            {
            }
        }
    }
}
