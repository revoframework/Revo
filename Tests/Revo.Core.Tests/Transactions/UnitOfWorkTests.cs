using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            sut = new UnitOfWork(innerTransactions, unitOfWorkListeners, publishEventBuffer);
        }

        [Fact]
        public void Constructor_BeginsWork()
        {
            unitOfWorkListeners[0].Received(1).OnWorkBegin(sut);
            unitOfWorkListeners[1].Received(1).OnWorkBegin(sut);
        }

        [Fact]
        public async Task CommitAsync_CommitsTxsFlushesEventsAndNotifies()
        {
            await sut.CommitAsync();

            Received.InOrder(() =>
            {
                unitOfWorkListeners[0].OnBeforeWorkCommitAsync(sut);
                unitOfWorkListeners[1].OnBeforeWorkCommitAsync(sut);

                innerTransactions[0].CommitAsync();
                innerTransactions[1].CommitAsync();

                publishEventBuffer.FlushAsync(Arg.Any<CancellationToken>());

                unitOfWorkListeners[0].OnWorkSucceededAsync(sut);
                unitOfWorkListeners[1].OnWorkSucceededAsync(sut);
            });
        }
    }
}
