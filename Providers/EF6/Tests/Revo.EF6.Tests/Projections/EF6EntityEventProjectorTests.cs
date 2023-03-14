using System;
using NSubstitute;
using Revo.Domain.Entities.EventSourcing;
using Revo.EF6.DataAccess.Entities;
using Revo.EF6.DataAccess.InMemory;
using Revo.EF6.Projections;

namespace Revo.EF6.Tests.Projections
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

        // TODO?

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
