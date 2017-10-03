using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Commands;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.EventSourcing;
using GTRevo.Infrastructure.Sagas;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Sagas
{
    public class SagaRepositoryTests
    {
        private readonly ICommandBus commandBus;
        private readonly ISagaMetadataRepository sagaMetadataRepository;
        private readonly IEventQueue eventQueue;
        private readonly IEventStore eventStore;
        private readonly IActorContext actorContext;
        private readonly IEntityTypeManager entityTypeManager;

        private readonly SagaRepository sut;

        public SagaRepositoryTests()
        {
            commandBus = Substitute.For<ICommandBus>();
            sagaMetadataRepository = Substitute.For<ISagaMetadataRepository>();
            eventQueue = Substitute.For<IEventQueue>();
            eventStore = Substitute.For<IEventStore>();
            actorContext = Substitute.For<IActorContext>();
            entityTypeManager = Substitute.For<IEntityTypeManager>();

            actorContext = Substitute.For<IActorContext>();
            actorContext.CurrentActorName.Returns("actor");

            Guid saga1ClassId = Guid.NewGuid();
            entityTypeManager.GetClrTypeByClassId(saga1ClassId)
                .Returns(typeof(Saga1));

            entityTypeManager.GetClassIdByClrType(typeof(Saga1))
                .Returns(saga1ClassId);

            sut = new SagaRepository(commandBus, eventStore, actorContext,
                entityTypeManager, eventQueue, new IRepositoryFilter[] {}, sagaMetadataRepository);
        }

        [Fact]
        public async Task SaveChangesAsync_SendsCommands()
        {
            Saga1 saga1 = new Saga1(Guid.NewGuid());
            saga1.Do();
            sut.Add(saga1);

            ICommand command = saga1.UncommitedCommands.First();

            await sut.SaveChangesAsync();

            commandBus.Received(1).Send(command);
        }

        [Fact]
        public async Task SaveChangesAsync_SetsAndSavesMetadata()
        {
            Saga1 saga1 = new Saga1(Guid.NewGuid());
            saga1.Do();
            sut.Add(saga1);

            await sut.SaveChangesAsync();

            sagaMetadataRepository.Received(1)
                .SetSagaMetadataAsync(saga1.Id, Arg.Is<SagaMetadata>(x =>  x.Keys.ToList().SequenceEqual(saga1.Keys)));
            sagaMetadataRepository.Received(1).SaveChangesAsync();
        }

        public class Saga1 : Saga
        {
            public Saga1(Guid id) : base(id)
            {
            }

            public void Do()
            {
                SendCommand(new Command1());
                SetKey("key", "value");
            }
        }

        public class Command1 : ICommand
        {
        }
    }
}
