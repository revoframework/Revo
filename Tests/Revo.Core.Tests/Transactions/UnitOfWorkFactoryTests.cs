using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Services;
using Revo.Core.Transactions;
using NSubstitute;
using Xunit;

namespace Revo.Core.Tests.Transactions
{
    public class UnitOfWorkFactoryTests
    {
        private readonly List<IUnitOfWorkListener> unitOfWorkListeners = new List<IUnitOfWorkListener>();
        private readonly UnitOfWorkFactory sut;
        private readonly IPublishEventBufferFactory publishEventBufferFactory;

        public UnitOfWorkFactoryTests()
        {
            CreateUnitOfWorkListener();
            CreateUnitOfWorkListener();

            publishEventBufferFactory = Substitute.For<IPublishEventBufferFactory>();

            var unitOfWorkListenerFactory = Substitute.For<IServiceFactory<IUnitOfWorkListener>>();
            unitOfWorkListenerFactory.GetServices().Returns(unitOfWorkListeners);

            sut = new UnitOfWorkFactory(unitOfWorkListenerFactory, publishEventBufferFactory);
        }

        [Fact]
        public void CreateUnitOfWork_NotifiesListeners()
        {
            using (var uow = sut.CreateUnitOfWork())
            {
                uow.Begin();
                unitOfWorkListeners[0].Received(1).OnWorkBegin(uow);
                unitOfWorkListeners[1].Received(1).OnWorkBegin(uow);
            }
        }

        [Fact]
        public async Task CreateUnitOfWork_CommitAsync_NotifiesListeners()
        {
            using (var uow = sut.CreateUnitOfWork())
            {
                uow.Begin();
                await uow.CommitAsync();

                unitOfWorkListeners[0].Received(1).OnWorkSucceededAsync(uow);
                unitOfWorkListeners[1].Received(1).OnWorkSucceededAsync(uow);
            }
        }
        
        private void CreateUnitOfWorkListener()
        {
            var listener = Substitute.For<IUnitOfWorkListener>();
            unitOfWorkListeners.Add(listener);
        }
    }
}