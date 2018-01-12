using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.EF6.EventStore;
using GTRevo.Testing.DataAccess.EF6;
using NSubstitute;

namespace GTRevo.Infrastructure.EF6.Tests.EventSourcing
{
    public class EF6EventStoreTests
    {
        private readonly EF6EventStore sut;
        private readonly FakeCrudRepository fakeCrudRepository;
        private readonly IDomainEventTypeCache domainEventTypeCache;

        public EF6EventStoreTests()
        {
            fakeCrudRepository = new FakeCrudRepository();
            domainEventTypeCache = Substitute.For<IDomainEventTypeCache>();

            sut = new EF6EventStore(fakeCrudRepository, domainEventTypeCache);
        }
    }
}
