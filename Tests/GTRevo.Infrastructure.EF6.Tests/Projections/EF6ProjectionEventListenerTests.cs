using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using GTRevo.Infrastructure.EF6.Projections;
using GTRevo.Infrastructure.Events.Async;
using GTRevo.Infrastructure.EventSourcing;
using GTRevo.Infrastructure.Projections;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.EF6.Tests.Projections
{
    public class ProjectionEventListenerTests
    {
        private readonly ProjectionEventListener sut;
        private readonly IEntityEventProjector<MyEntity1> myEntity1Projector;
        private readonly IEventSourcedAggregateRepository eventSourcedRepository;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IServiceLocator serviceLocator;

        public ProjectionEventListenerTests()
        {
            eventSourcedRepository = Substitute.For<IEventSourcedAggregateRepository>();
            entityTypeManager = Substitute.For<IEntityTypeManager>();

            myEntity1Projector = Substitute.For<IEntityEventProjector<MyEntity1>>();

            serviceLocator = Substitute.For<IServiceLocator>();
            serviceLocator.GetAll(typeof(IEF6EntityEventProjector<MyEntity1>)).Returns(new object[] { myEntity1Projector });

            sut = Substitute.ForPartsOf<EF6ProjectionEventListener>(eventSourcedRepository, entityTypeManager,
                serviceLocator, new EF6ProjectionEventListener.EF6ProjectionEventSequencer());
        }

        [Fact]
        public void GetProjectors_ReturnsProjectorsForType()
        {
            var result = sut.GetProjectors(typeof(MyEntity1)).ToList();
            result.Should().HaveCount(1);
            result.Should().Contain(myEntity1Projector);
        }
        

        public class MyEntity1 : AggregateRoot, IEventSourcedAggregateRoot
        {
            public static Guid ClassId = Guid.NewGuid();

            public MyEntity1(Guid id) : base(id)
            {
            }

            public void LoadState(AggregateState state)
            {
            }
        }
    }
}
