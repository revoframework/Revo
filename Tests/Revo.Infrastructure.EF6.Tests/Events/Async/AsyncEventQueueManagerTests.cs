using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.DataAccess.EF6.Entities;
using Revo.Infrastructure.EF6.Events.Async;
using NSubstitute;
using Revo.DataAccess.InMemory;
using Revo.Domain.Events;
using Xunit;

namespace Revo.Infrastructure.EF6.Tests.Events.Async
{
    public class AsyncEventQueueManagerTests
    {
        private AsyncEventQueueManager sut;
        private InMemoryCrudRepository crudRepository;
        private IDomainEventTypeCache domainEventTypeCache;

        public AsyncEventQueueManagerTests()
        {
            crudRepository = new InMemoryCrudRepository();
            domainEventTypeCache = Substitute.For<IDomainEventTypeCache>();

            sut = new AsyncEventQueueManager(crudRepository, domainEventTypeCache);
        }

        [Fact]
        public async Task A()
        {
            
        }
    }
}
