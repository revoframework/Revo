using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Commands;
using Revo.Infrastructure.Sagas;
using NSubstitute;
using Revo.Core.Transactions;
using Revo.Domain.Entities;
using Revo.Domain.Sagas;
using Revo.Testing.Infrastructure.Repositories;
using Xunit;

namespace Revo.Infrastructure.Tests.Sagas
{
    public class SagaRepositoryTests
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICommandBus commandBus;
        private readonly ISagaMetadataRepository sagaMetadataRepository;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly FakeRepository repository;
        private readonly Guid saga1ClassId = Guid.NewGuid();
        private readonly SagaRepository sut;

        private ITransaction uowInnerTransaction = null;

        public SagaRepositoryTests()
        {
            unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.When(x => x.AddInnerTransaction(Arg.Any<ITransaction>())).Do(ci => uowInnerTransaction = ci.ArgAt<ITransaction>(0));

            commandBus = Substitute.For<ICommandBus>();
            sagaMetadataRepository = Substitute.For<ISagaMetadataRepository>();
            entityTypeManager = Substitute.For<IEntityTypeManager>();
            repository = Substitute.ForPartsOf<FakeRepository>();
            entityTypeManager = Substitute.For<IEntityTypeManager>();

            entityTypeManager.GetClassInfoByClassId(saga1ClassId)
                .Returns(new DomainClassInfo(saga1ClassId, null, typeof(Saga1)));

            entityTypeManager.GetClassInfoByClrType(typeof(Saga1))
                .Returns(new DomainClassInfo(saga1ClassId, null, typeof(Saga1)));
            
            sut = new SagaRepository(commandBus, repository, sagaMetadataRepository, entityTypeManager, unitOfWork);
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
                .Should().Throw<ArgumentException>();

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
        public async Task SendSagaCommandsAsync_SendsCommands()
        {
            Saga1 saga1 = new Saga1(Guid.NewGuid());
            saga1.Do();
            sut.Add(saga1);

            ICommandBase command = saga1.UncommitedCommands.First();

            await sut.SendSagaCommandsAsync();

            commandBus.Received(1).SendAsync(command);
        }

        [Fact]
        public async Task Constructor_AddsMetadataSaveTransactionToUoW()
        {
            unitOfWork.Received(1).AddInnerTransaction(Arg.Any<ITransaction>());
            uowInnerTransaction.Should().NotBeNull();

            Saga1 saga1 = new Saga1(Guid.NewGuid());
            saga1.Do();
            sut.Add(saga1);

            IEnumerable<KeyValuePair<string, string>> sagaKeys = null;
            sagaMetadataRepository.When(x => x.SetSagaKeysAsync(saga1.Id, Arg.Any<IEnumerable<KeyValuePair<string, string>>>())).Do(
                ci => sagaKeys = ci.ArgAt<IEnumerable<KeyValuePair<string, string>>>(1));

            await uowInnerTransaction.CommitAsync();

            sagaMetadataRepository.Received(1).SaveChangesAsync();

            Received.InOrder(() =>
            {
                sagaMetadataRepository.SetSagaKeysAsync(saga1.Id, Arg.Any<IEnumerable<KeyValuePair<string, string>>>());
                sagaMetadataRepository.SaveChangesAsync();
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
                Send(new Command1());
                SetSagaKey("key", "value");
            }
        }

        public class Command1 : ICommand
        {
        }
    }
}
