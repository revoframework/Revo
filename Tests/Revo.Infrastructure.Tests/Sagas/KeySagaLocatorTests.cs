using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Infrastructure.Sagas;
using Revo.Testing.Infrastructure;
using NSubstitute;
using Revo.Domain.Entities;
using Revo.Domain.Entities.Attributes;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Domain.Sagas;
using Xunit;

namespace Revo.Infrastructure.Tests.Sagas
{
    public class KeySagaLocatorTests
    {
        private readonly ISagaMetadataRepository sagaMetadataRepository;
        private readonly ISagaRegistry sagaRegistry;
        private readonly KeySagaLocator sut;
        
        public KeySagaLocatorTests()
        {
            sagaRegistry = Substitute.For<ISagaRegistry>();
            sagaMetadataRepository = Substitute.For<ISagaMetadataRepository>();

            sut = new KeySagaLocator(sagaRegistry, sagaMetadataRepository);
        }

        [Fact]
        public async Task LocateSagasAsync_AlwaysStarting_StartsSaga()
        {
            sagaRegistry.LookupRegistrations(typeof(Event1))
                .Returns(new List<SagaEventRegistration>()
                {
                    SagaEventRegistration.AlwaysStarting(typeof(Saga1), typeof(Event1))
                });
            
            var result = await sut.LocateSagasAsync(
                new Event1() { Foo = 5 }.ToMessageDraft());

            result.Should().BeEquivalentTo(new[]
            {
                LocatedSaga.CreateNew(typeof(Saga1))
            });
        }

        [Fact]
        public async Task LocateSagasAsync_FindsExistingSagaByKey()
        {
            sagaRegistry.LookupRegistrations(typeof(Event1))
                .Returns(new List<SagaEventRegistration>()
                {
                    SagaEventRegistration.MatchedByKey(typeof(Saga1), typeof(Event1),
                        x => ((Event1) x).Foo.ToString(), "foo", false)
                });

            Guid sagaId1 = Guid.NewGuid();
            
            sagaMetadataRepository.FindSagasByKeyAsync(typeof(Saga1).GetClassId(), "foo", "5")
                .Returns(new [] { new SagaMatch() { Id = sagaId1, ClassId = typeof(Saga1).GetClassId() } });

            var result = await sut.LocateSagasAsync(new Event1() { Foo = 5 }.ToMessageDraft());

            result.Should().BeEquivalentTo(new[]
            {
                LocatedSaga.FromId(sagaId1, typeof(Saga1))
            });
        }
        
        [Fact]
        public async Task LocateSagasAsync_StartsSagaWhenNotFoundByKey()
        {
            sagaRegistry.LookupRegistrations(typeof(Event1))
                .Returns(new List<SagaEventRegistration>()
                {
                    SagaEventRegistration.MatchedByKey(typeof(Saga1), typeof(Event1),
                        x => ((Event1) x).Foo.ToString(), "foo", true)
                });
            
            var result = await sut.LocateSagasAsync(new Event1() { Foo = 5 }.ToMessageDraft());

            result.Should().BeEquivalentTo(new[]
            {
                LocatedSaga.CreateNew(typeof(Saga1))
            });
        }

        [Fact]
        public async Task LocateSagasAsync_DeliversToAll()
        {
            sagaRegistry.LookupRegistrations(typeof(Event1))
                .Returns(new List<SagaEventRegistration>()
                {
                    SagaEventRegistration.ToAllExistingInstances(typeof(Saga1), typeof(Event1), true)
                });

            Guid sagaId1 = Guid.NewGuid();

            sagaMetadataRepository.FindSagasAsync(typeof(Saga1).GetClassId())
                .Returns(new[] { new SagaMatch() { Id = sagaId1, ClassId = typeof(Saga1).GetClassId() } });

            var result = await sut.LocateSagasAsync(new Event1().ToMessageDraft());

            result.Should().BeEquivalentTo(new[]
            {
                LocatedSaga.FromId(sagaId1, typeof(Saga1))
            });
        }

        [Fact]
        public async Task LocateSagasAsync_DeliverToAllStartsNewWhenNoSagas()
        {
            sagaRegistry.LookupRegistrations(typeof(Event1))
                .Returns(new List<SagaEventRegistration>()
                {
                    SagaEventRegistration.ToAllExistingInstances(typeof(Saga1), typeof(Event1), true)
                });

            Guid sagaId1 = Guid.NewGuid();

            sagaMetadataRepository.FindSagasAsync(typeof(Saga1).GetClassId())
                .Returns(new SagaMatch[] { });

            var result = await sut.LocateSagasAsync(new Event1().ToMessageDraft());

            result.Should().BeEquivalentTo(new[]
            {
                LocatedSaga.CreateNew(typeof(Saga1))
            });
        }

        [Fact]
        public async Task LocateSagasAsync_DistinctWhenMultipleMatchesPerSaga()
        {
            sagaRegistry.LookupRegistrations(typeof(Event1))
                .Returns(new List<SagaEventRegistration>()
                {
                    SagaEventRegistration.MatchedByKey(typeof(Saga1), typeof(Event1),
                        x => ((Event1) x).Foo.ToString(), "foo", false),
                    SagaEventRegistration.MatchedByKey(typeof(Saga1), typeof(Event1),
                        x => ((Event1) x).Bar.ToString(), "bar", false)
                });

            Guid sagaId1 = Guid.NewGuid();

            sagaMetadataRepository.FindSagasByKeyAsync(typeof(Saga1).GetClassId(), "foo", "5")
                .Returns(new[] { new SagaMatch() { Id = sagaId1, ClassId = typeof(Saga1).GetClassId() } });
            sagaMetadataRepository.FindSagasByKeyAsync(typeof(Saga1).GetClassId(), "bar", "42")
                .Returns(new[] { new SagaMatch() { Id = sagaId1, ClassId = typeof(Saga1).GetClassId() } });

            var result = await sut.LocateSagasAsync(new Event1() { Foo = 5, Bar = 42 }.ToMessageDraft());

            result.Should().BeEquivalentTo(new[]
            {
                LocatedSaga.FromId(sagaId1, typeof(Saga1))
            });
        }

        [DomainClassId("2C7348E9-B7DE-4C26-BB1D-33464F86DADE")]
        public class Saga1 : ISaga
        {
            public Saga1(Guid id)
            {
                Id = id;
            }

            public Guid Id { get; set; }
            public bool IsChanged => UncommittedEvents.Any() || UncommitedCommands.Any();
            public IReadOnlyCollection<DomainAggregateEvent> UncommittedEvents { get; set; }
            public int Version { get; set; }
            public bool IsDeleted { get; set; }

            public void Commit()
            {
                Version++;
                UncommittedEvents = new List<DomainAggregateEvent>();
                UncommitedCommands = new List<ICommand>();
            }

            public void LoadState(AggregateState state)
            {
            }

            public IEnumerable<ICommandBase> UncommitedCommands { get; set; }
            public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Keys { get; set; } = new Dictionary<string, IReadOnlyCollection<string>>();
            public bool IsEnded { get; }

            public List<IEventMessage<DomainEvent>> HandledEvents { get; set; } = new List<IEventMessage<DomainEvent>>();

            public virtual void HandleEvent(IEventMessage<DomainEvent> ev)
            {
                HandledEvents.Add(ev);
            }
        }

        [DomainClassId("2F6B0532-D4CA-4C8E-AFFD-40AC732317D2")]
        public class Saga2 : Saga1
        {
            public Saga2(Guid id) : base(id)
            {
            }

            public override void HandleEvent(IEventMessage<DomainEvent> ev)
            {
                base.HandleEvent(ev);

                if (ev is IEventMessage<Event1> event1)
                {
                    Keys = new Dictionary<string, IReadOnlyCollection<string>>()
                    {
                        {"foo", new List<string>() { event1.Event.Foo.ToString() }}
                    };
                }
                else
                {
                    Keys = new Dictionary<string, IReadOnlyCollection<string>>();
                }
            }
        }

        public class Event1 : DomainEvent
        {
            public int Foo { get; set; }
            public int Bar { get; set; }
        }
    }
}
