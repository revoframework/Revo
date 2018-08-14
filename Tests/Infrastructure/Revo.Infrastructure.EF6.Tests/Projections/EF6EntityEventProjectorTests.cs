using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.EF6.InMemory;
using Revo.Domain.Entities.EventSourcing;
using Revo.Infrastructure.EF6.Projections;
using Xunit;

namespace Revo.Infrastructure.EF6.Tests.Projections
{
    public class EF6EntityEventProjectorTests
    {
        private TestProjector sut;
        private EF6InMemoryCrudRepository ef6CrudRepository;

        public EF6EntityEventProjectorTests()
        {
            ef6CrudRepository = Substitute.ForPartsOf<EF6InMemoryCrudRepository>();
            sut = new TestProjector(ef6CrudRepository);
        }

        [Fact]
        public async Task CommitChangesAsync_SavesRepository()
        {
            await sut.CommitChangesAsync();
            ef6CrudRepository.ReceivedWithAnyArgs(1).SaveChangesAsync();
        }

        public class TestAggregate : EventSourcedAggregateRoot
        {
            public TestAggregate(Guid id) : base(id)
            {
            }
        }

        public class TestProjector : EF6EntityEventProjector<TestAggregate>
        {
            public TestProjector(IEF6CrudRepository repository) : base(repository)
            {
            }
        }
    }
}
