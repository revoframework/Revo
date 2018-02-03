using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Domain.ReadModel;
using Revo.Domain.Tenancy;
using Revo.Infrastructure.Tenancy;
using NSubstitute;
using Xunit;

namespace Revo.Infrastructure.Tests.Tenancy
{
    public class TenantRepositoryFilterTests
    {
        private readonly TenantRepositoryFilter sut;
        private readonly ITenantContext tenantContext;
        private readonly List<TestEntity> entities = new List<TestEntity>();
        private readonly List<TestEntity2> entitiesUnfiltered = new List<TestEntity2>();
        private readonly Guid tenantId1 = Guid.NewGuid();
        private readonly Guid tenantId2 = Guid.NewGuid();
        private readonly ITenant tenant;

        public TenantRepositoryFilterTests()
        {
            tenantContext = Substitute.For<ITenantContext>();

            entities.AddRange(new[]
            {
                new TestEntity() {Id = Guid.NewGuid(), TenantId = null},
                new TestEntity() {Id = Guid.NewGuid(), TenantId = tenantId1},
                new TestEntity() {Id = Guid.NewGuid(), TenantId = tenantId2}
            });

            entitiesUnfiltered.AddRange(new[]
            {
                new TestEntity2() {Id = Guid.NewGuid()}
            });

            tenant = Substitute.For<ITenant>();

            sut = new TenantRepositoryFilter(tenantContext);
        }

        [Fact]
        public void FilterResults_NullContextReturnsAll()
        {
            tenantContext.Tenant.Returns((ITenant)null);

            var results = sut.FilterResults(entities.AsQueryable());
            Assert.True(results.SequenceEqual(entities));
        }

        [Fact]
        public void FilterResults_ReturnsOwnAndCommonOnly()
        {
            tenant.Id.Returns(tenantId1);
            tenantContext.Tenant.Returns(tenant);

            var results = sut.FilterResults(entities.AsQueryable());
            Assert.True(results.SequenceEqual(
                entities.Where(x => x.TenantId == tenantId1 || x.TenantId == null)));
        }

        [Fact]
        public void FilterResults_ReturnsUnfiltered()
        {
            tenant.Id.Returns(tenantId1);
            tenantContext.Tenant.Returns(tenant);

            var results = sut.FilterResults(entitiesUnfiltered.AsQueryable());
            Assert.True(results.SequenceEqual(entitiesUnfiltered));
        }

        [Fact]
        public void FilterResult_ReturnsOwn()
        {
            tenant.Id.Returns(tenantId1);
            tenantContext.Tenant.Returns(tenant);

            var result = sut.FilterResult(entities[1]);
            Assert.Equal(entities[1], result);
        }

        [Fact]
        public void FilterResult_ReturnsCommon()
        {
            tenant.Id.Returns(tenantId1);
            tenantContext.Tenant.Returns(tenant);

            var result = sut.FilterResult(entities[0]);
            Assert.Equal(entities[0], result);
        }

        [Fact]
        public void FilterResult_ReturnUnfiltered()
        {
            tenant.Id.Returns(tenantId1);
            tenantContext.Tenant.Returns(tenant);

            var result = sut.FilterResult(entitiesUnfiltered[0]);
            Assert.Equal(entitiesUnfiltered[0], result);
        }
        
        [Fact]
        public void FilterResult_ReturnsNullWhenForeign()
        {
            tenant.Id.Returns(tenantId1);
            tenantContext.Tenant.Returns(tenant);

            var result = sut.FilterResult(entities[2]);
            Assert.Equal(null, result);
        }

        [Fact]
        public void FilterResult_NullContextReturnsOwned()
        {
            tenantContext.Tenant.Returns((ITenant)null);

            var result = sut.FilterResult(entities[1]);
            Assert.Equal(entities[1], result);
        }

        [Fact]
        public void FilterResult_NullContextReturnsCommon()
        {
            tenantContext.Tenant.Returns((ITenant)null);

            var result = sut.FilterResult(entities[0]);
            Assert.Equal(entities[0], result);
        }
        
        [Fact]
        public void FilterAdded_AllowsOwnAndCommon()
        {
            tenant.Id.Returns(tenantId1);
            tenantContext.Tenant.Returns(tenant);

            sut.FilterAdded(entities[0]);
            sut.FilterAdded(entities[1]);
        }

        [Fact]
        public void FilterAdded_AllowsUnfiltered()
        {
            tenant.Id.Returns(tenantId1);
            tenantContext.Tenant.Returns(tenant);
            
            sut.FilterAdded(entitiesUnfiltered[0]);
        }

        [Fact]
        public void FilterAdded_ThrowsWhenForeign()
        {
            tenant.Id.Returns(tenantId1);
            tenantContext.Tenant.Returns(tenant);

            Assert.Throws<InvalidOperationException>(() =>
            {
                sut.FilterAdded(entities[2]);
            });
        }

        [Fact]
        public void FilterAdded_NullContextAllowsAny()
        {
            tenantContext.Tenant.Returns((ITenant)null);

            foreach (var entity in entities)
            {
                sut.FilterAdded(entity);
            }
        }

        [Fact]
        public void FilterModified_AllowsOwnAndCommon()
        {
            tenant.Id.Returns(tenantId1);
            tenantContext.Tenant.Returns(tenant);

            sut.FilterModified(entities[0]);
            sut.FilterModified(entities[1]);
        }

        [Fact]
        public void FilterModified_AllowsUnfiltered()
        {
            tenant.Id.Returns(tenantId1);
            tenantContext.Tenant.Returns(tenant);

            sut.FilterModified(entitiesUnfiltered[0]);
        }

        [Fact]
        public void FilterModified_ThrowsWhenForeign()
        {
            tenant.Id.Returns(tenantId1);
            tenantContext.Tenant.Returns(tenant);

            Assert.Throws<InvalidOperationException>(() =>
            {
                sut.FilterModified(entities[2]);
            });
        }

        [Fact]
        public void FilterModified_NullContextAllowsAny()
        {
            tenantContext.Tenant.Returns((ITenant)null);

            foreach (var entity in entities)
            {
                sut.FilterModified(entity);
            }
        }

        public class TestEntity : TenantEntityReadModel
        {
        }

        public class TestEntity2 : EntityReadModel
        {
        }
    }
}
