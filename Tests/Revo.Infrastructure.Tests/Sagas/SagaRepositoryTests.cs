using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Revo.Infrastructure.Repositories;
using Revo.Testing.Infrastructure.Repositories;
using Xunit;

namespace Revo.Infrastructure.Tests.Sagas
{
    public class SagaRepositoryTests
    {
        private readonly ICommandBus commandBus;
        private readonly ISagaMetadataRepository sagaMetadataRepository;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly FakeRepository repository;

        private readonly SagaRepository sut;


        private readonly Guid saga1ClassId = Guid.NewGuid();

        public SagaRepositoryTests()
        {
            commandBus = Substitute.For<ICommandBus>();
            sagaMetadataRepository = Substitute.For<ISagaMetadataRepository>();
            entityTypeManager = Substitute.For<IEntityTypeManager>();
            repository = Substitute.ForPartsOf<FakeRepository>();
            entityTypeManager = Substitute.For<IEntityTypeManager>();

            entityTypeManager.GetClrTypeByClassId(saga1ClassId)
                .Returns(typeof(Saga1));

            entityTypeManager.GetClassIdByClrType(typeof(Saga1))
                .Returns(saga1ClassId);
            
            sut = new SagaRepository(commandBus, repository, sagaMetadataRepository, entityTypeManager);
        }

        [Fact]
        public void MetadataRepository_GetsRepo()
        {
            sut.MetadataRepository.Should().Be(sagaMetadataRepository);
        }

        [Fact]
        public void Add_AddsToMetadataRepo()
        {
            Saga1 saga = new Saga1(Guid.NewGuid());
            sut.Add(saga);

            sagaMetadataRepository.Received(1).AddSaga(saga.Id, saga1ClassId);
        }

        [Fact]
        public void Add_AddsToRepo()
        {
            Saga1 saga = new Saga1(Guid.NewGuid());
            sut.Add(saga);

            repository.Received(1).Add(saga);
        }

        [Fact]
        public void Add_ThrowsForDuplicateIds()
        {
            Saga1 saga = new Saga1(Guid.NewGuid());
            Saga1 saga2 = new Saga1(saga.Id);
            sut.Add(saga);

            sut.Invoking(x => x.Add(saga2))
                .ShouldThrow<ArgumentException>();

            repository.Received(1).Add(saga);
        }

        [Fact]
        public void Add_TwiceIsNoop()
        {
            Saga1 saga = new Saga1(Guid.NewGuid());
            sut.Add(saga);
            sut.Add(saga);

            sagaMetadataRepository.Received(1).AddSaga(saga.Id, saga1ClassId);
            repository.Received(1).Add(saga);
        }

        [Fact]
        public async Task GetAsync_GetsFromRepo()
        {
            Saga1 saga = new Saga1(Guid.NewGuid());
            repository.Aggregates.Add(new FakeRepository.EntityEntry(saga, FakeRepository.EntityState.Unchanged));

            var result = await sut.GetAsync<Saga1>(saga.Id);
            result.Should().Be(saga);
        }

        [Fact]
        public async Task GetAsync_ByClassIdGetsFromRepo()
        {
            Saga1 saga = new Saga1(Guid.NewGuid());
            repository.Aggregates.Add(new FakeRepository.EntityEntry(saga, FakeRepository.EntityState.Unchanged));

            var result = await sut.GetAsync(saga.Id, saga1ClassId);
            result.Should().Be(saga);
        }

        [Fact]
        public async Task SaveChangesAsync_SendsCommands()
        {
            Saga1 saga1 = new Saga1(Guid.NewGuid());
            saga1.Do();
            sut.Add(saga1);

            ICommandBase command = saga1.UncommitedCommands.First();

            await sut.SaveChangesAsync();

            commandBus.Received(1).SendAsync(command);
        }

        [Fact]
        public async Task SaveChangesAsync_PublishesCommandsSavesSagasAndMetadataInOrder()
        {
            var saga = new Saga1(Guid.NewGuid());
            saga.Do();
            sut.Add(saga);
            await sut.SaveChangesAsync();

            Received.InOrder(() =>
            {
                commandBus.SendAsync(Arg.Any<ICommandBase>(), Arg.Any<CancellationToken>());
                repository.SaveChangesAsync();
                sagaMetadataRepository.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task SaveChangesAsync_SetsAndSavesMetadata()
        {
            Saga1 saga1 = new Saga1(Guid.NewGuid());
            saga1.Do();
            sut.Add(saga1);

            IEnumerable<KeyValuePair<string, string>> sagaKeys = null;
            sagaMetadataRepository.When(x => x.SetSagaKeysAsync(saga1.Id, Arg.Any<IEnumerable<KeyValuePair<string, string>>>())).Do(
                ci => sagaKeys = ci.ArgAt<IEnumerable<KeyValuePair<string, string>>>(1));

            await sut.SaveChangesAsync();

            Received.InOrder(() =>
            {
                sagaMetadataRepository.Received(1).SetSagaKeysAsync(saga1.Id, Arg.Any<IEnumerable<KeyValuePair<string, string>>>());
                sagaMetadataRepository.Received(1).SaveChangesAsync();
            });

            sagaKeys.Should()
                .BeEquivalentTo(saga1.Keys.SelectMany(x =>
                    x.Value.Select(y => new KeyValuePair<string, string>(x.Key, y))));
        }

        public class Saga1 : EventSourcedSaga
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
