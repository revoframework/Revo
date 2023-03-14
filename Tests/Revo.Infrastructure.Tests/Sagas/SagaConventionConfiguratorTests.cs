using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Revo.Infrastructure.Sagas;
using NSubstitute;
using Revo.Domain.Events;
using Revo.Domain.Sagas;
using Xunit;

namespace Revo.Infrastructure.Tests.Sagas
{
    public class SagaConventionConfiguratorTests
    {
        private readonly ConventionSagaConfigurator sut;
        private readonly ISagaConventionConfigurationCache sagaConventionConfigurationCache;

        public SagaConventionConfiguratorTests()
        {
            sagaConventionConfigurationCache = Substitute.For<ISagaConventionConfigurationCache>();
            sut = new ConventionSagaConfigurator(sagaConventionConfigurationCache);
        }

        [Fact]
        public void ConfigureSagas_AddsToRegistry_AlwaysStarting()
        {
            sagaConventionConfigurationCache.ConfigurationInfos.Returns(
                    new Dictionary<Type, SagaConfigurationInfo>()
                    {
                        {
                            typeof(Saga1),
                            new SagaConfigurationInfo(
                                new Dictionary<Type, IReadOnlyCollection<SagaConventionEventInfo>>()
                                {
                                    {typeof(Event1), new[] {new SagaConventionEventInfo((saga, domainEvent) => { })}}
                                })
                        }
                    });

            var sagaRegistry = Substitute.For<ISagaRegistry>();
            sut.ConfigureSagas(sagaRegistry);

            sagaRegistry.Received(1).Add(Arg.Is<SagaEventRegistration>(x =>
                x.IsAlwaysStarting && x.SagaType == typeof(Saga1) && x.EventType == typeof(Event1)));
        }

        [Fact]
        public void ConfigureSagas_AddsToRegistry_KeyCorrelatedEvent()
        {
            Func<DomainEvent, string> eventKeyExpression = (domainEvent) => "key";

            sagaConventionConfigurationCache.ConfigurationInfos.Returns(
                new ReadOnlyDictionary<Type, SagaConfigurationInfo>(
                    new Dictionary<Type, SagaConfigurationInfo>()
                    {
                        {
                            typeof(Saga1), new SagaConfigurationInfo(
                                new Dictionary<Type, IReadOnlyCollection<SagaConventionEventInfo>>()
                                {
                                    {
                                        typeof(Event1),
                                        new[]
                                        {
                                            new SagaConventionEventInfo(eventKeyExpression, "key", false,
                                                (saga, domainEvent) => { })
                                        }
                                    }
                                })
                        }
                    }));

            var sagaRegistry = Substitute.For<ISagaRegistry>();
            sut.ConfigureSagas(sagaRegistry);

            sagaRegistry.Received(1).Add(Arg.Is<SagaEventRegistration>(x =>
                !x.IsAlwaysStarting && x.SagaType == typeof(Saga1) && x.EventType == typeof(Event1)
                && x.EventKeyExpression == eventKeyExpression && x.IsStartingIfSagaNotFound == false
                && x.SagaKey == "key"));
        }

        public class Saga1 : EventSourcedSaga
        {
            public Saga1(Guid id) : base(id)
            {
            }
        }

        public class Event1 : DomainEvent
        {
        }
    }
}
