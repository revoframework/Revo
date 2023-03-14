using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Events;
using Revo.Domain.Events;
using Revo.Domain.Sagas;
using Revo.Domain.Sagas.Attributes;
using Revo.Infrastructure.Repositories;
using Revo.Infrastructure.Sagas;
using Revo.Testing.Infrastructure;
using Xunit;

namespace Revo.Infrastructure.Tests.Sagas
{
    public class SagaEventDispatcherTests
    {
        private readonly SagaEventDispatcher sut;
        private readonly IEntityFactory entityFactory;
        private readonly ISagaLocator[] sagaLocators;
        private readonly ISagaRepository sagaRepository;

        public SagaEventDispatcherTests()
        {
            entityFactory = Substitute.For<IEntityFactory>();
            sagaLocators = new[]
            {
                Substitute.For<ISagaLocator>(),
                Substitute.For<ISagaLocator>()
            };
            sagaRepository = Substitute.For<ISagaRepository>();


            sut = new SagaEventDispatcher(sagaLocators, sagaRepository, entityFactory);
        }

        [Fact]
        public async Task DispatchEventsToSagas_DispatchesToExistingSaga()
        {
            var events = new List<IEventMessage<DomainEvent>>()
            {
                new Event1().ToMessageDraft()
            };

            Saga1 saga = new Saga1(Guid.Parse("20D27705-FFB8-49F5-8331-99E3654B5B19"));
            sagaLocators[0].LocateSagasAsync(events[0]).Returns(new[] {LocatedSaga.FromId(saga.Id, typeof(Saga1))});
            sagaRepository.GetAsync<Saga1>(saga.Id).Returns(saga);

            await sut.DispatchEventsToSagas(events);

            saga.HandledEvents.Should().BeEquivalentTo(events);
            sagaRepository.Received(1).SendSagaCommandsAsync();
        }

        [Fact]
        public async Task DispatchEventsToSagas_InvokesSagaOnlyOnceWhenMatchedByMultipleLocators()
        {
            var events = new List<IEventMessage<DomainEvent>>()
            {
                new Event1().ToMessageDraft()
            };

            Saga1 saga = new Saga1(Guid.Parse("20D27705-FFB8-49F5-8331-99E3654B5B19"));
            sagaLocators[0].LocateSagasAsync(events[0]).Returns(new[] {LocatedSaga.FromId(saga.Id, typeof(Saga1))});
            sagaLocators[1].LocateSagasAsync(events[0]).Returns(new[] {LocatedSaga.FromId(saga.Id, typeof(Saga1))});
            sagaRepository.GetAsync<Saga1>(saga.Id).Returns(saga);

            await sut.DispatchEventsToSagas(events);

            saga.HandledEvents.Should().BeEquivalentTo(events);
            sagaRepository.Received(1).SendSagaCommandsAsync();
        }

        [Fact]
        public async Task DispatchEventsToSagas_CreatesNewSaga()
        {
            var events = new List<IEventMessage<DomainEvent>>()
            {
                new Event1().ToMessageDraft()
            };

            Saga1 saga = null;
            sagaLocators[0].LocateSagasAsync(events[0]).Returns(new[] {LocatedSaga.CreateNew(typeof(Saga1))});
            entityFactory.ConstructEntity(typeof(Saga1), Arg.Any<Guid>()).Returns(ci =>
            {
                return saga = new Saga1(ci.ArgAt<Guid>(1));
            });

            await sut.DispatchEventsToSagas(events);

            saga.Should().NotBeNull();
            saga.HandledEvents.Should().BeEquivalentTo(events);
            sagaRepository.Received(1).SendSagaCommandsAsync();
        }

        public class Event1 : DomainEvent
        {
        }

        public class Saga1 : EventSourcedSaga
        {
            public Saga1(Guid id) : base(id)
            {
            }

            public List<IEventMessage<DomainEvent>> HandledEvents = new List<IEventMessage<DomainEvent>>();

            [SagaEvent(IsAlwaysStarting = true)]
            private void Handle(IEventMessage<Event1> ev)
            {
                HandledEvents.Add(ev);
            }
        }
    }
}
