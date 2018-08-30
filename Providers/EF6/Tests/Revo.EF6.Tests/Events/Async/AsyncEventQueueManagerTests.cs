using System.Threading.Tasks;
using NSubstitute;
using Revo.DataAccess.InMemory;
using Revo.EF6.Events;
using Revo.EF6.Events.Async;
using Xunit;

namespace Revo.EF6.Tests.Events.Async
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
