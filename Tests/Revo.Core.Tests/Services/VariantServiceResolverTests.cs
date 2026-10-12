using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Configuration;
using Revo.Core.Services;
using Xunit;

namespace Revo.Core.Tests.Services
{
    public class VariantServiceResolverTests
    {
        private readonly ServiceCollection services = new();

        [Fact]
        public void GetServices_ReturnsExactRegistrationsFirstThenContravariantOnes()
        {
            services.AddTransient<IListener<IBaseEvent>, InterfaceListener>();
            services.AddTransient<IListener<DerivedEvent>, DerivedListener>();

            var listeners = CreateResolver().GetServices(typeof(IListener<DerivedEvent>));

            listeners.Select(x => x.GetType()).Should().Equal(typeof(DerivedListener), typeof(InterfaceListener));
        }

        [Fact]
        public void GetServices_ReturnsVariantRegistrationsInRegistrationOrder()
        {
            services.AddTransient<IListener<BaseEvent>, BaseListener>();
            services.AddTransient<IListener<object>, ObjectListener>();
            services.AddTransient<IListener<IBaseEvent>, InterfaceListener>();

            var listeners = CreateResolver().GetServices(typeof(IListener<DerivedEvent>));

            listeners.Select(x => x.GetType()).Should().Equal(
                typeof(BaseListener), typeof(ObjectListener), typeof(InterfaceListener));
        }

        [Fact]
        public void GetServices_IncludesOpenGenericRegistrations()
        {
            services.AddTransient(typeof(IListener<>), typeof(GenericListener<>));
            services.AddTransient<IListener<BaseEvent>, BaseListener>();

            var listeners = CreateResolver().GetServices(typeof(IListener<DerivedEvent>));

            listeners.Select(x => x.GetType()).Should().Equal(
                typeof(GenericListener<DerivedEvent>), typeof(BaseListener));
        }

        [Fact]
        public void GetServices_KeepsExplicitClosedRegistrationOfOpenGenericImplementation()
        {
            services.AddTransient(typeof(IListener<>), typeof(GenericListener<>));
            services.AddTransient<IListener<BaseEvent>, GenericListener<BaseEvent>>();

            var listeners = CreateResolver().GetServices(typeof(IListener<DerivedEvent>));

            listeners.Select(x => x.GetType()).Should().Equal(
                typeof(GenericListener<DerivedEvent>), typeof(GenericListener<BaseEvent>));
        }

        [Fact]
        public void GetServices_ReturnsEachRegistrationOfVariantType()
        {
            services.AddTransient<IListener<BaseEvent>, BaseListener>();
            services.AddTransient<IListener<BaseEvent>, BaseListener2>();

            var listeners = CreateResolver().GetServices(typeof(IListener<DerivedEvent>));

            listeners.Select(x => x.GetType()).Should().Equal(typeof(BaseListener), typeof(BaseListener2));
        }

        [Fact]
        public void GetServices_DoesNotReturnIncompatibleRegistrations()
        {
            services.AddTransient<IListener<DerivedEvent>, DerivedListener>();
            services.AddTransient<IListener<OtherEvent>, OtherListener>();

            var listeners = CreateResolver().GetServices(typeof(IListener<BaseEvent>));

            listeners.Should().BeEmpty();
        }

        [Fact]
        public void GetServices_DoesNotApplyVarianceToInvariantTypes()
        {
            services.AddTransient<IInvariantListener<BaseEvent>, InvariantBaseListener>();

            var listeners = CreateResolver().GetServices(typeof(IInvariantListener<DerivedEvent>));

            listeners.Should().BeEmpty();
        }

        [Fact]
        public void GetServices_KeepsInvariantTypeArgumentsInvariant()
        {
            services.AddTransient<IQueryListener<BaseEvent, int>, IntQueryListener>();

            var resolver = CreateResolver();

            resolver.GetServices(typeof(IQueryListener<DerivedEvent, int>))
                .Select(x => x.GetType()).Should().Equal(typeof(IntQueryListener));
            resolver.GetServices(typeof(IQueryListener<DerivedEvent, long>)).Should().BeEmpty();
        }

        [Fact]
        public void GetServices_ResolvesNonGenericTypesExactly()
        {
            services.AddTransient<INonGenericService, NonGenericService>();

            var resolved = CreateResolver().GetServices(typeof(INonGenericService));

            resolved.Select(x => x.GetType()).Should().Equal(typeof(NonGenericService));
        }

        [Fact]
        public void GetServices_IgnoresKeyedRegistrations()
        {
            services.AddKeyedTransient<IListener<BaseEvent>, BaseListener>("key");

            var listeners = CreateResolver().GetServices(typeof(IListener<DerivedEvent>));

            listeners.Should().BeEmpty();
        }

        [Fact]
        public void GetServices_IncludesRegistrationsAddedAfterIndexCreation()
        {
            var index = new VariantServiceTypeIndex(services);
            services.AddTransient<IListener<BaseEvent>, BaseListener>();

            var resolver = new VariantServiceResolver(services.BuildServiceProvider(), index);
            var listeners = resolver.GetServices(typeof(IListener<DerivedEvent>));

            listeners.Select(x => x.GetType()).Should().Equal(typeof(BaseListener));
        }

        [Fact]
        public void GetService_PrefersExactRegistration()
        {
            services.AddTransient<IListener<BaseEvent>, BaseListener>();
            services.AddTransient<IListener<DerivedEvent>, DerivedListener>();

            var listener = CreateResolver().GetService(typeof(IListener<DerivedEvent>));

            listener.Should().BeOfType<DerivedListener>();
        }

        [Fact]
        public void GetService_FallsBackToSingleVariantRegistration()
        {
            services.AddTransient<IListener<BaseEvent>, BaseListener>();

            var listener = CreateResolver().GetService(typeof(IListener<DerivedEvent>));

            listener.Should().BeOfType<BaseListener>();
        }

        [Fact]
        public void GetService_ThrowsWhenVariantRegistrationsAreAmbiguous()
        {
            services.AddTransient<IListener<BaseEvent>, BaseListener>();
            services.AddTransient<IListener<IBaseEvent>, InterfaceListener>();

            var resolver = CreateResolver();

            resolver.Invoking(x => x.GetService(typeof(IListener<DerivedEvent>)))
                .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetService_ReturnsNullWhenNotRegistered()
        {
            CreateResolver().GetService(typeof(IListener<DerivedEvent>)).Should().BeNull();
        }

        [Fact]
        public void GetRequiredService_ThrowsWhenNotRegistered()
        {
            var resolver = CreateResolver();

            resolver.Invoking(x => x.GetRequiredService(typeof(IListener<DerivedEvent>)))
                .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void AddRevo_ResolverAndServiceFactoryResolveFromCallingScope()
        {
            services.AddLogging();
            services.AddRevo();
            services.AddScoped<IListener<BaseEvent>, BaseListener>();

            using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            using var scope1 = provider.CreateScope();
            using var scope2 = provider.CreateScope();

            var scope1Listener = scope1.ServiceProvider.GetRequiredService<IVariantServiceResolver>()
                .GetServices<IListener<DerivedEvent>>().Single();
            var scope1FactoryListener = scope1.ServiceProvider
                .GetRequiredService<IServiceFactory<IListener<DerivedEvent>>>()
                .GetServices().Single();
            var scope2Listener = scope2.ServiceProvider.GetRequiredService<IVariantServiceResolver>()
                .GetServices<IListener<DerivedEvent>>().Single();

            scope1Listener.Should().BeOfType<BaseListener>();
            scope1FactoryListener.Should().BeSameAs(scope1Listener);
            scope2Listener.Should().NotBeSameAs(scope1Listener);
        }

        private VariantServiceResolver CreateResolver()
        {
            return new VariantServiceResolver(services.BuildServiceProvider(), new VariantServiceTypeIndex(services));
        }

        public interface IListener<in T>
        {
        }

        public interface IInvariantListener<T>
        {
        }

        public interface IQueryListener<in TQuery, TResult>
        {
        }

        public interface INonGenericService
        {
        }

        public interface IBaseEvent
        {
        }

        public class BaseEvent : IBaseEvent
        {
        }

        public class DerivedEvent : BaseEvent
        {
        }

        public class OtherEvent
        {
        }

        public class BaseListener : IListener<BaseEvent>
        {
        }

        public class BaseListener2 : IListener<BaseEvent>
        {
        }

        public class DerivedListener : IListener<DerivedEvent>
        {
        }

        public class InterfaceListener : IListener<IBaseEvent>
        {
        }

        public class ObjectListener : IListener<object>
        {
        }

        public class OtherListener : IListener<OtherEvent>
        {
        }

        public class GenericListener<T> : IListener<T>
        {
        }

        public class InvariantBaseListener : IInvariantListener<BaseEvent>
        {
        }

        public class IntQueryListener : IQueryListener<BaseEvent, int>
        {
        }

        public class NonGenericService : INonGenericService
        {
        }
    }
}
