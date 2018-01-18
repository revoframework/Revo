using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using NSubstitute;
using Xunit;

namespace GTRevo.Core.Tests.Transactions
{
    public class UnitOfWorkFactoryTests
    {
        private readonly List<IUnitOfWorkProvider> transactionProviders = new List<IUnitOfWorkProvider>();
        private readonly List<Tuple<ITransactionProvider, ITransaction>> transactions = new List<Tuple<ITransactionProvider, ITransaction>>();
        private readonly List<IUnitOfWorkListener> unitOfWorkListeners = new List<IUnitOfWorkListener>();
        private readonly UnitOfWorkFactory sut;
        private readonly Func<IPublishEventBuffer> publishEventBufferFunc;

        public UnitOfWorkFactoryTests()
        {
            CreateTransactionProvider();
            CreateTransactionProvider();

            CreateUnitOfWorkListener();
            CreateUnitOfWorkListener();

            publishEventBufferFunc = () => Substitute.For<IPublishEventBuffer>();

            sut = new UnitOfWorkFactory(transactionProviders.ToArray(), unitOfWorkListeners.ToArray(), publishEventBufferFunc);
        }

        // TODO merge these tests with UnitOfWorkTests?

        [Fact]
        public void CreateUnitOfWork_CreatesAllSubtransactions()
        {
            using (var uow = sut.CreateUnitOfWork())
            {
                Assert.Equal(2, transactions.Count);
                Assert.Equal(transactionProviders[0], transactions[0].Item1);
                Assert.Equal(transactionProviders[1], transactions[1].Item1);
            }
        }

        [Fact]
        public async Task CreateUnitOfWork_CommitAsync_CommitsAllSubtransactions()
        {
            using (var uow = sut.CreateUnitOfWork())
            {
                await uow.CommitAsync();

                transactions[0].Item2.Received(1).CommitAsync();
                transactions[1].Item2.Received(1).CommitAsync();
            }
        }

        [Fact]
        public void CreateUnitOfWork_NotifiesListeners()
        {
            using (var uow = sut.CreateUnitOfWork())
            {
                unitOfWorkListeners[0].Received(1).OnWorkBegin(uow);
                unitOfWorkListeners[1].Received(1).OnWorkBegin(uow);
            }
        }

        [Fact]
        public async Task CreateUnitOfWork_CommitAsync_NotifiesListeners()
        {
            using (var uow = sut.CreateUnitOfWork())
            {
                await uow.CommitAsync();

                unitOfWorkListeners[0].Received(1).OnWorkSucceededAsync(uow);
                unitOfWorkListeners[1].Received(1).OnWorkSucceededAsync(uow);
            }
        }

        private IUnitOfWorkProvider CreateTransactionProvider()
        {
            var txProvider = Substitute.For<IUnitOfWorkProvider>();
            txProvider.CreateTransaction().Returns(callInfo => CreateTransaction(txProvider));

            transactionProviders.Add(txProvider);

            return txProvider;
        }

        private ITransaction CreateTransaction(ITransactionProvider transactionProvider)
        {
            var tx = Substitute.For<ITransaction>();
            transactions.Add(new Tuple<ITransactionProvider, ITransaction>(transactionProvider, tx));
            return tx;
        }

        private IUnitOfWorkListener CreateUnitOfWorkListener()
        {
            var listener = Substitute.For<IUnitOfWorkListener>();
            unitOfWorkListeners.Add(listener);
            return listener;
        }
    }
}
