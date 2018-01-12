using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Core.Transactions;
using NSubstitute;
using Xunit;

namespace GTRevo.Platform.Tests.Transactions
{
    public class UnitOfWorkTests
    {
        private readonly List<IUnitOfWorkProvider> transactionProviders = new List<IUnitOfWorkProvider>();
        private readonly List<Tuple<ITransactionProvider, ITransaction>> transactions = new List<Tuple<ITransactionProvider, ITransaction>>();
        private readonly List<IUnitOfWorkListener> unitOfWorkListeners = new List<IUnitOfWorkListener>();
        private readonly IUnitOfWorkFactory sut;

        public UnitOfWorkTests()
        {
            CreateTransactionProvider();
            CreateTransactionProvider();

            CreateUnitOfWorkListener();
            CreateUnitOfWorkListener();

            sut = new UnitOfWorkFactory(transactionProviders.ToArray(), unitOfWorkListeners.ToArray());
        }

        [Fact]
        public void CreateTransaction_CreatesAllSubtransactions()
        {
            using (var tx = sut.CreateTransaction())
            {
                Assert.Equal(2, transactions.Count);
                Assert.Equal(transactionProviders[0], transactions[0].Item1);
                Assert.Equal(transactionProviders[1], transactions[1].Item1);
            }
        }

        [Fact]
        public async Task CreateTransaction_CommitAsync_CommitsAllSubtransactions()
        {
            using (var tx = sut.CreateTransaction())
            {
                await tx.CommitAsync();

                transactions[0].Item2.Received(1).CommitAsync();
                transactions[1].Item2.Received(1).CommitAsync();
            }
        }

        [Fact]
        public void CreateTransaction_NotifiesListeners()
        {
            using (var tx = sut.CreateTransaction())
            {
                unitOfWorkListeners[0].Received(1).OnTransactionBegin(tx);
                unitOfWorkListeners[1].Received(1).OnTransactionBegin(tx);
            }
        }

        [Fact]
        public async Task CreateTransaction_CommitAsync_NotifiesListeners()
        {
            using (var tx = sut.CreateTransaction())
            {
                await tx.CommitAsync();

                unitOfWorkListeners[0].Received(1).OnTransactionSucceededAsync(tx);
                unitOfWorkListeners[1].Received(1).OnTransactionSucceededAsync(tx);
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
