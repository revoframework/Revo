using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Infrastructure.EF6.Projections;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.EventSourcing;
using Revo.Infrastructure.Projections;
using NSubstitute;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Xunit;

namespace Revo.Infrastructure.EF6.Tests.Projections
{
    public class ProjectionEventListenerTests
    {
        private readonly ProjectionEventListener sut;
        private readonly IEntityEventProjector<MyEntity1> myEntity1Projector;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IServiceLocator serviceLocator;

        public ProjectionEventListenerTests()
        {
            entityTypeManager = Substitute.For<IEntityTypeManager>();

            myEntity1Projector = Substitute.For<IEntityEventProjector<MyEntity1>>();

            serviceLocator = Substitute.For<IServiceLocator>();
            serviceLocator.GetAll(typeof(IEF6EntityEventProjector<MyEntity1>)).Returns(new object[] { myEntity1Projector });

            sut = Substitute.ForPartsOf<EF6ProjectionEventListener>(entityTypeManager,
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
