using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Tenancy;
using GTRevo.Infrastructure.Tenancy;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Tenancy
{
    public class TenantRepositoryFilterTests
    {
        private readonly TenantRepositoryFilter sut;
        private readonly ITenantContext tenantContext;
        private readonly List<TestEntity> entities = new List<TestEntity>();
        private readonly Guid tenantId1 = Guid.NewGuid();
        private readonly Guid tenantId2 = Guid.NewGuid();

        public TenantRepositoryFilterTests()
        {
            tenantContext = Substitute.For<ITenantContext>();

            entities.AddRange(new[]
            {
                new TestEntity() { Id = Guid.NewGuid(), TenantId = null },
                new TestEntity() { Id = Guid.NewGuid(), TenantId = tenantId1 },
                new TestEntity() { Id = Guid.NewGuid(), TenantId = tenantId2 }
            });

            sut = new TenantRepositoryFilter(tenantContext);
        }

        [Fact]
        public void FilterResults_NullContextReturnsAll()
        {
            tenantContext.TenantId.Returns((Guid?)null);

            var results = sut.FilterResults(entities.AsQueryable());
            Assert.True(results.SequenceEqual(entities));
        }

        [Fact]
        public void FilterResults_ReturnsTenantsAndCommonOnly()
        {
            tenantContext.TenantId.Returns(tenantId1);

            var results = sut.FilterResults(entities.AsQueryable());
            Assert.True(results.SequenceEqual(entities.Where(x => x.TenantId == tenantId1 || x.TenantId == null)));
        }

        public class TestEntity : TenantEntityReadModel
        {
        }
    }
}
