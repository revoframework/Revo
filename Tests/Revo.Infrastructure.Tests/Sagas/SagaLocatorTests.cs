using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Infrastructure.Sagas;
using Revo.Testing.Infrastructure;
using NSubstitute;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Domain.Sagas;
using Xunit;

namespace Revo.Infrastructure.Tests.Sagas
{
    public class SagaLocatorTests
    {
        private readonly ISagaRepository sagaRepository;
        private readonly ISagaRegistry sagaRegistry;
        private readonly SagaLocator sut;

        public SagaLocatorTests()
        {
            sagaRegistry = Substitute.For<ISagaRegistry>();
            sagaRepository = Substitute.For<ISagaRepository>();
            sut = new SagaLocator(sagaRegistry, sagaRepository);
        }

        [Fact]
        public async Task LocateAndDispatchAsync_AlwaysStarting_StartsSaga()
        {
            sagaRegistry.LookupRegistrations(typeof(Event1))
                .Returns(new List<SagaEventRegistration>()
                {
                    new SagaEventRegistration(typeof(Saga1), typeof(Event1))
                });
            
            var events = new List<IEventMessage<DomainEvent>>()
            {
                new Event1() { Foo = 5 }.ToMessageDraft()
            };

            List<ISaga> addedSagas = new List<ISaga>();
            sagaRepository.When(x => x.Add(Arg.Any<ISaga>())).Do(ci => addedSagas.Add(ci.ArgAt<ISaga>(0)));

            await sut.LocateAndDispatchAsync(events);

            Assert.Equal(1, addedSagas.Count);
            Assert.Equal(typeof(Saga1), addedSagas[0].GetType());
            Assert.Equal(1, ((Saga1) addedSagas[0]).HandledEvents.Count);
            Assert.Equal(events[0], ((Saga1) addedSagas[0]).HandledEvents[0]);

            sagaRepository.ReceivedWithAnyArgs(1).SaveChangesAsync();
        }

        [Fact]
        public async Task LocateAndDispatchAsync_FindsExistingSaga()
        {
            sagaRegistry.LookupRegistrations(typeof(Event1))
                .Returns(new List<SagaEventRegistration>()
                {
                    new SagaEventRegistration(typeof(Saga1), typeof(Event1), x => ((Event1) x).Foo.ToString(),
                        "foo", false)
                });

            var events = new List<IEventMessage<DomainEvent>>()
            {
                new Event1() { Foo = 5 }.ToMessageDraft()
            };

            Saga1 saga1 = new Saga1(Guid.NewGuid());

            ISagaMetadataRepository sagaMetadataRepository = Substitute.For<ISagaMetadataRepository>();
            sagaMetadataRepository.FindSagaIdsByKeyAsync("foo", "5").Returns(new Guid[] {saga1.Id});
            sagaRepository.MetadataRepository.Returns(sagaMetadataRepository);
            sagaRepository.GetAsync(saga1.Id).Returns(saga1);

            await sut.LocateAndDispatchAsync(events);
            
            Assert.Equal(1, saga1.HandledEvents.Count);
            Assert.Equal(events[0], saga1.HandledEvents[0]);

            sagaRepository.ReceivedWithAnyArgs(1).SaveChangesAsync();
        }
        
        [Fact]
        public async Task LocateAndDispatchAsync_StartsSagaWhenNotFound()
        {
            sagaRegistry.LookupRegistrations(typeof(Event1))
                .Returns(new List<SagaEventRegistration>()
                {
                    new SagaEventRegistration(typeof(Saga1), typeof(Event1), x => ((Event1) x).Foo.ToString(),
                        "foo", true)
                });

            var events = new List<IEventMessage<DomainEvent>>()
            {
                new Event1() { Foo = 5 }.ToMessageDraft()
            };

            List<ISaga> addedSagas = new List<ISaga>();
            sagaRepository.When(x => x.Add(Arg.Any<ISaga>())).Do(ci => addedSagas.Add(ci.ArgAt<ISaga>(0)));

            await sut.LocateAndDispatchAsync(events);

            Assert.Equal(1, addedSagas.Count);
            Assert.Equal(typeof(Saga1), addedSagas[0].GetType());
            Assert.Equal(1, ((Saga1)addedSagas[0]).HandledEvents.Count);
            Assert.Equal(events[0], ((Saga1)addedSagas[0]).HandledEvents[0]);

            sagaRepository.ReceivedWithAnyArgs(1).SaveChangesAsync();
        }

        [Fact]
        public async Task LocateAndDispatchAsync_FindsNewlyCreatedSaga()
        {
            sagaRegistry.LookupRegistrations(typeof(Event1))
                .Returns(new List<SagaEventRegistration>()
                {
                    new SagaEventRegistration(typeof(Saga2), typeof(Event1), x => ((Event1) x).Foo.ToString(),
                        "foo", true)
                });

            var events = new List<IEventMessage<DomainEvent>>()
            {
                new Event1() { Foo = 5 }.ToMessageDraft(),
                new Event1() { Foo = 5 }.ToMessageDraft()
            };

            List<ISaga> addedSagas = new List<ISaga>();
            sagaRepository.When(x => x.Add(Arg.Any<ISaga>())).Do(ci => addedSagas.Add(ci.ArgAt<ISaga>(0)));

            await sut.LocateAndDispatchAsync(events);

            Assert.Equal(1, addedSagas.Count);
            Assert.Equal(typeof(Saga2), addedSagas[0].GetType());
            Assert.Equal(2, ((Saga2)addedSagas[0]).HandledEvents.Count);
            Assert.Equal(events[0], ((Saga2)addedSagas[0]).HandledEvents[0]);
            Assert.Equal(events[1], ((Saga2)addedSagas[0]).HandledEvents[1]);

            sagaRepository.ReceivedWithAnyArgs(1).SaveChangesAsync();
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

            public IEnumerable<ICommand> UncommitedCommands { get; set; }
            public IReadOnlyDictionary<string, string> Keys { get; set; } = new Dictionary<string, string>();
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
                    Keys = new Dictionary<string, string>()
                    {
                        {"foo", event1.Event.Foo.ToString()}
                    };
                }
                else
                {
                    Keys = new Dictionary<string, string>();
                }
            }
        }

        public class Event1 : DomainEvent
        {
            public int Foo { get; set; }
        }
    }
}
