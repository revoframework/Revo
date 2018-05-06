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
using Revo.Infrastructure.EF6.Events;
using Xunit;

namespace Revo.Infrastructure.EF6.Tests.Events.Async
{
    public class AsyncEventQueueManagerTests
    {
        private AsyncEventQueueManager sut;
        private InMemoryCrudRepository crudRepository;
        private IEventSerializer eventSerializer;

        public AsyncEventQueueManagerTests()
        {
            crudRepository = new InMemoryCrudRepository();
            eventSerializer = Substitute.For<IEventSerializer>();

            sut = new AsyncEventQueueManager(crudRepository, eventSerializer);
        }

        [Fact]
        public async Task A()
        {
            
        }
    }
}
