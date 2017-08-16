using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Tenancy;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Tenancy
{
    public class SingleTenantContextResolverTests
    {
        private readonly SingleTenantContextResolver sut;
        private readonly Guid tenantId = Guid.NewGuid();

        public SingleTenantContextResolverTests()
        {
            sut = new SingleTenantContextResolver(tenantId);
        }

        [Fact]
        public void TenantId_ReturnsCorrectId()
        {
            Assert.Equal(tenantId, sut.TenantId);
        }
    }
}
