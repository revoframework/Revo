using System;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Transactions;
using NSubstitute;
using Xunit;

namespace Revo.Core.Tests.Transactions
{
    public class UnitOfWorkTests
    {
        private UnitOfWork sut;
        private ITransaction[] innerTransactions;
        private IUnitOfWorkListener[] unitOfWorkListeners;
        private IPublishEventBuffer publishEventBuffer;

        public UnitOfWorkTests()
        {
            innerTransactions = new[] { Substitute.For<ITransaction>(), Substitute.For<ITransaction>() };
            unitOfWorkListeners = new[] { Substitute.For<IUnitOfWorkListener>(), Substitute.For<IUnitOfWorkListener>() };
            publishEventBuffer = Substitute.For<IPublishEventBuffer>();

            sut = new UnitOfWork(new Lazy<IUnitOfWorkListener[]>(unitOfWorkListeners), publishEventBuffer);
        }

        [Fact]
        public void Begin_Notifies()
        {
            sut.Begin();

            unitOfWorkListeners[0].Received(1).OnWorkBegin(sut);
            unitOfWorkListeners[1].Received(1).OnWorkBegin(sut);
        }

        [Fact]
        public async Task CommitAsync_CommitsInnerTransactions()
        {
            sut.Begin();
            sut.AddInnerTransaction(innerTransactions[0]);
            sut.AddInnerTransaction(innerTransactions[1]);

            await sut.CommitAsync();

            innerTransactions[0].Received(1).CommitAsync();
            innerTransactions[1].Received(1).CommitAsync();
        }

        [Fact]
        public async Task CommitAsync_NotifiesListenersBefore()
        {
            sut.Begin();
            sut.AddInnerTransaction(innerTransactions[0]);
            await sut.CommitAsync();

            Received.InOrder(() =>
            {
                unitOfWorkListeners[0].OnBeforeWorkCommitAsync(sut);
                innerTransactions[0].CommitAsync();
            });

            Received.InOrder(() =>
            {
                unitOfWorkListeners[1].OnBeforeWorkCommitAsync(sut);
                innerTransactions[0].CommitAsync();
            });
        }

        [Fact]
        public async Task CommitAsync_FlushesEvents()
        {
            sut.Begin();
            sut.AddInnerTransaction(innerTransactions[0]);
            sut.AddInnerTransaction(innerTransactions[1]);
            await sut.CommitAsync();

            publishEventBuffer.Received(1).FlushAsync(Arg.Any<CancellationToken>());

            Received.InOrder(() =>
            {
                innerTransactions[0].CommitAsync();
                publishEventBuffer.FlushAsync(Arg.Any<CancellationToken>());
            });

            Received.InOrder(() =>
            {
                innerTransactions[1].CommitAsync();
                publishEventBuffer.FlushAsync(Arg.Any<CancellationToken>());
            });
        }

        [Fact]
        public async Task CommitAsync_NotifiesListenersWhenSucceeded()
        {
            sut.Begin();
            sut.AddInnerTransaction(innerTransactions[0]);
            await sut.CommitAsync();

            Received.InOrder(() =>
            {
                publishEventBuffer.FlushAsync(Arg.Any<CancellationToken>());
                unitOfWorkListeners[0].OnWorkSucceededAsync(sut);
            });

            Received.InOrder(() =>
            {
                publishEventBuffer.FlushAsync(Arg.Any<CancellationToken>());
                unitOfWorkListeners[1].OnWorkSucceededAsync(sut);
            });
        }
    }
}
