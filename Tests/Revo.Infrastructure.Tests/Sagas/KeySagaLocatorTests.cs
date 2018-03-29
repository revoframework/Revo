using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Infrastructure.Sagas;
using Revo.Testing.Infrastructure;
using NSubstitute;
using Revo.Domain.Entities;
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
        private readonly IEntityTypeManager entityTypeManager;
        private readonly KeySagaLocator sut;

        private readonly Guid saga1ClassId = Guid.NewGuid();

        public KeySagaLocatorTests()
        {
            sagaRegistry = Substitute.For<ISagaRegistry>();
            sagaMetadataRepository = Substitute.For<ISagaMetadataRepository>();

            entityTypeManager = Substitute.For<IEntityTypeManager>();
            entityTypeManager.GetClrTypeByClassId(saga1ClassId).Returns(typeof(Saga1));
            entityTypeManager.GetClassIdByClrType(typeof(Saga1)).Returns(saga1ClassId);

            sut = new KeySagaLocator(sagaRegistry, sagaMetadataRepository, entityTypeManager);
        }

        [Fact]
        public async Task LocateSagasAsync_AlwaysStarting_StartsSaga()
        {
            sagaRegistry.LookupRegistrations(typeof(Event1))
                .Returns(new List<SagaEventRegistration>()
                {
                    new SagaEventRegistration(typeof(Saga1), typeof(Event1))
                });
            
            var result = await sut.LocateSagasAsync(
                new Event1() { Foo = 5 }.ToMessageDraft());

            result.Should().BeEquivalentTo(new[]
            {
                new LocatedSaga(typeof(Saga1))
            });
        }

        [Fact]
        public async Task LocateSagasAsync_FindsExistingSaga()
        {
            sagaRegistry.LookupRegistrations(typeof(Event1))
                .Returns(new List<SagaEventRegistration>()
                {
                    new SagaEventRegistration(typeof(Saga1), typeof(Event1), x => ((Event1) x).Foo.ToString(),
                        "foo", false)
                });

            Guid sagaId1 = Guid.NewGuid();
            
            sagaMetadataRepository.FindSagasByKeyAsync("foo", "5")
                .Returns(new [] { new SagaKeyMatch() { Id = sagaId1, ClassId = saga1ClassId} });

            var result = await sut.LocateSagasAsync(new Event1() { Foo = 5 }.ToMessageDraft());

            result.Should().BeEquivalentTo(new[]
            {
                new LocatedSaga(sagaId1, typeof(Saga1))
            });
        }
        
        [Fact]
        public async Task LocateSagasAsync_StartsSagaWhenNotFound()
        {
            sagaRegistry.LookupRegistrations(typeof(Event1))
                .Returns(new List<SagaEventRegistration>()
                {
                    new SagaEventRegistration(typeof(Saga1), typeof(Event1), x => ((Event1) x).Foo.ToString(),
                        "foo", true)
                });
            
            var result = await sut.LocateSagasAsync(new Event1() { Foo = 5 }.ToMessageDraft());

            result.Should().BeEquivalentTo(new[]
            {
                new LocatedSaga(typeof(Saga1))
            });
        }

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
        }
    }
}
