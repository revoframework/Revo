using FluentAssertions;
using NSubstitute;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.DataAccess.InMemory;
using Revo.Domain.Entities;
using Revo.Domain.Entities.Attributes;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Domain.Tenancy;
using Revo.Domain.Tenancy.Events;
using Revo.Infrastructure.Projections;
using Revo.Testing.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Revo.Infrastructure.Tests.Projections
{
    public static class CrudEntityEventToPocoProjectorTests
    {
        public static Guid AggregateClassId = Guid.Parse("7C836285-7B76-49F5-9D1D-275EA724F5FB");

        public class WithConventions
        {
            private readonly TestCrudEntityEventToPocoProjector sut;
            private readonly ICrudRepository crudRepository;
            private readonly List<IEventMessageDraft<DomainAggregateEvent>> events;

            public WithConventions()
            {
                crudRepository = Substitute.ForPartsOf<InMemoryCrudRepository>();
                sut = new TestCrudEntityEventToPocoProjector(crudRepository);

                events = new DomainAggregateEvent[]
                {
                    new TestEvent1()
                }.ToAggregateEventMessages(AggregateClassId);
            }

            [Fact]
            public async Task ProjectEventsAsync_AddsToRepository()
            {
                await sut.ProjectEventsAsync(Guid.NewGuid(), events);
                crudRepository.FindAllWithAdded<TestReadModel>().Should().HaveCount(1);
            }

            [Fact]
            public async Task CreateProjectionTargetAsync_SetsEntityId()
            {
                Guid aggregateId = Guid.Parse("7C836285-7B76-49F5-9D1D-275EA724F5FB");
                await sut.ProjectEventsAsync(aggregateId, events);
                crudRepository.FindAllWithAdded<TestReadModel>().First().Id.Should().Be(aggregateId);
            }

            [Fact]
            public async Task CreateProjectionTargetAsync_SetsEntityClassId()
            {
                await sut.ProjectEventsAsync(Guid.NewGuid(), events);
                crudRepository.FindAllWithAdded<TestReadModel>().First().ClassId.Should().Be(AggregateClassId);
            }
            
            [Fact]
            public async Task CreateProjectionTargetAsync_SetsEntityClassIdFromAggregateAttribute()
            {
                var eventsWoClassId = new DomainAggregateEvent[]
                {
                    new TestEvent1()
                }.ToAggregateEventMessages(null);

                await sut.ProjectEventsAsync(Guid.NewGuid(), eventsWoClassId);
                crudRepository.FindAllWithAdded<TestReadModel>().First().ClassId.Should().Be(typeof(TestAggregate).GetClassId());
            }

            [Fact]
            public async Task CreateProjectionTargetAsync_SetsEntityTenantFromEvent()
            {
                Guid tenantId = Guid.Parse("7C836285-7B76-49F5-9D1D-275EA724F5FB");
                var tenantEvent = new TenantAggregateRootCreated(tenantId).ToMessageDraft();
                tenantEvent.SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, (events.Last().Metadata.GetStreamSequenceNumber() + 1).ToString());
                events.Add(tenantEvent);
                await sut.ProjectEventsAsync(Guid.NewGuid(), events);
                crudRepository.FindAllWithAdded<TestReadModel>().First().TenantId.Should().Be(tenantId);
            }

            [Fact]
            public async Task CreateProjectionTargetAsync_SetsEntityTenantFromEventMetadata()
            {
                Guid tenantId = Guid.Parse("7C836285-7B76-49F5-9D1D-275EA724F5FB");
                events.First().SetMetadata(BasicEventMetadataNames.AggregateTenantId, tenantId.ToString());
                await sut.ProjectEventsAsync(Guid.NewGuid(), events);
                crudRepository.FindAllWithAdded<TestReadModel>().First().TenantId.Should().Be(tenantId);
            }
            
            public class TestReadModel : TenantClassEntityReadModel
            {
            }

            public class TestCrudEntityEventToPocoProjector : CrudEntityEventToPocoProjector<TestAggregate, TestReadModel>
            {
                public TestCrudEntityEventToPocoProjector(ICrudRepository crudRepository) : base(crudRepository)
                {
                }
            }

        }

        public class WithSimpleReadModel
        {
            private readonly TestSimpleCrudEntityEventToPocoProjector sut;
            private readonly ICrudRepository crudRepository;
            private readonly List<IEventMessageDraft<DomainAggregateEvent>> events;

            public WithSimpleReadModel()
            {
                crudRepository = Substitute.ForPartsOf<InMemoryCrudRepository>();
                sut = new TestSimpleCrudEntityEventToPocoProjector(crudRepository);

                events = new DomainAggregateEvent[]
                {
                    new TestEvent1()
                }.ToAggregateEventMessages(AggregateClassId);
            }

            [Fact]
            public async Task ProjectEventsAsync_ProjectsWithoutPrefillingData()
            {
                var tenantEvent = new TenantAggregateRootCreated(Guid.NewGuid()).ToMessageDraft();
                tenantEvent.SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, (events.Last().Metadata.GetStreamSequenceNumber() + 1).ToString());
                events.Add(tenantEvent);

                await sut.ProjectEventsAsync(Guid.NewGuid(), events);
                crudRepository.FindAllWithAdded<TestSimpleReadModel>().Should().HaveCount(1);
            }

            public class TestSimpleReadModel
            {
            }

            public class TestSimpleCrudEntityEventToPocoProjector : CrudEntityEventToPocoProjector<TestAggregate, TestSimpleReadModel>
            {
                public TestSimpleCrudEntityEventToPocoProjector(ICrudRepository crudRepository) : base(crudRepository)
                {
                }
            }
        }
        
        [DomainClassId("100D0877-206B-48A3-AD46-F3E52E56F993")]
        public class TestAggregate : EventSourcedAggregateRoot
        {
            public TestAggregate(Guid id) : base(id)
            {
            }
        }

        public class TestEvent1 : DomainAggregateEvent
        {
        }
    }
}
