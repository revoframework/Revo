using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Core;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.EF6.Projections;
using Revo.Infrastructure.Projections;
using Xunit;

namespace Revo.EF6.Tests.Projections
{
    public class EF6ProjectionEventListenerTests
    {
        private readonly ProjectionEventListener sut;
        private readonly IEntityEventProjector<MyEntity1> myEntity1Projector;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IServiceLocator serviceLocator;

        public EF6ProjectionEventListenerTests()
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
