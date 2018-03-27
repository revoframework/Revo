using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MoreLinq;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventSourcing;
using Revo.Infrastructure.EventStore;
using Revo.Infrastructure.Sagas;
using NSubstitute;
using Revo.Domain.Entities;
using Revo.Domain.Sagas;
using Xunit;

namespace Revo.Infrastructure.Tests.Sagas
{
    public class SagaRepositoryTests
    {
        private readonly ICommandBus commandBus;
        private readonly ISagaMetadataRepository sagaMetadataRepository;
        private readonly IPublishEventBuffer publishEventBuffer;
        private readonly IEventStore eventStore;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IEventMessageFactory eventMessageFactory;

        private readonly SagaRepository sut;

        public SagaRepositoryTests()
        {
            commandBus = Substitute.For<ICommandBus>();
            sagaMetadataRepository = Substitute.For<ISagaMetadataRepository>();
            publishEventBuffer = Substitute.For<IPublishEventBuffer>();
            eventStore = Substitute.For<IEventStore>();
            entityTypeManager = Substitute.For<IEntityTypeManager>();

            Guid saga1ClassId = Guid.NewGuid();
            entityTypeManager.GetClrTypeByClassId(saga1ClassId)
                .Returns(typeof(Saga1));

            entityTypeManager.GetClassIdByClrType(typeof(Saga1))
                .Returns(saga1ClassId);

            eventMessageFactory = Substitute.For<IEventMessageFactory>();
            eventMessageFactory.CreateMessageAsync(null).ReturnsForAnyArgs(ci =>
            {
                var @event = ci.ArgAt<IEvent>(0);
                Type messageType = typeof(EventMessageDraft<>).MakeGenericType(@event.GetType());
                IEventMessageDraft messageDraft = (IEventMessageDraft)messageType.GetConstructor(new[] { @event.GetType() }).Invoke(new[] { @event });
                messageDraft.SetMetadata("TestKey", "TestValue");
                return messageDraft;
            }); // TODO something more lightweight?

            sut = new SagaRepository(commandBus, eventStore, entityTypeManager,
                publishEventBuffer, new IRepositoryFilter[] {}, eventMessageFactory, sagaMetadataRepository);
        }

        [Fact]
        public async Task SaveChangesAsync_SendsCommands()
        {
            Saga1 saga1 = new Saga1(Guid.NewGuid());
            saga1.Do();
            sut.Add(saga1);

            ICommand command = saga1.UncommitedCommands.First();

            await sut.SaveChangesAsync();

            commandBus.Received(1).SendAsync(command);
        }

        [Fact]
        public async Task SaveChangesAsync_SetsAndSavesMetadata()
        {
            Saga1 saga1 = new Saga1(Guid.NewGuid());
            saga1.Do();
            sut.Add(saga1);

            SagaMetadata sagaMetadata = null;
            sagaMetadataRepository.When(x => x.SetSagaMetadataAsync(saga1.Id, Arg.Any<SagaMetadata>())).Do(
                ci => sagaMetadata = ci.ArgAt<SagaMetadata>(1));

            await sut.SaveChangesAsync();

            sagaMetadataRepository.Received(1).SetSagaMetadataAsync(saga1.Id, Arg.Any<SagaMetadata>());
            sagaMetadataRepository.Received(1).SaveChangesAsync();

            sagaMetadata.Keys.Count.ShouldBeEquivalentTo(saga1.Keys.Count);
            sagaMetadata.Keys.ForEach(x =>
                {
                    saga1.Keys.Keys.Should().Contain(x.Key);
                    saga1.Keys[x.Key].Should().BeEquivalentTo(x.Value);
                });
        }

        public class Saga1 : Saga
        {
            public Saga1(Guid id) : base(id)
            {
            }

            public void Do()
            {
                SendCommand(new Command1());
                SetSagaKey("key", "value");
            }
        }

        public class Command1 : ICommand
        {
        }
    }
}
