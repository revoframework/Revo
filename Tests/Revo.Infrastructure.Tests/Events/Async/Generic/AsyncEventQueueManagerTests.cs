using System.Threading.Tasks;
using NSubstitute;
using Revo.DataAccess.InMemory;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async.Generic;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Async.Generic
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
    }
}
