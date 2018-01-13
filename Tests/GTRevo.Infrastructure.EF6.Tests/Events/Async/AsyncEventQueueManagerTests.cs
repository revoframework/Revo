using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.DataAccess.EF6.Entities;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.EF6.Events.Async;
using GTRevo.Testing.DataAccess;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.EF6.Tests.Events.Async
{
    public class AsyncEventQueueManagerTests
    {
        private AsyncEventQueueManager sut;
        private FakeCrudRepository crudRepository;
        private IDomainEventTypeCache domainEventTypeCache;

        public AsyncEventQueueManagerTests()
        {
            crudRepository = new FakeCrudRepository();
            domainEventTypeCache = Substitute.For<IDomainEventTypeCache>();

            sut = new AsyncEventQueueManager(crudRepository, domainEventTypeCache);
        }

        [Fact]
        public async Task A()
        {
            
        }
    }
}
