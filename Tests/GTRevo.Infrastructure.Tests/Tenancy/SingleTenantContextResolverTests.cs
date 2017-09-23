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
    public class SingleTenantContextResolverTests
    {
        private readonly SingleTenantContextResolver sut;
        private readonly ITenantManager tenantManager;
        private readonly ITenant tenant;

        public SingleTenantContextResolverTests()
        {
            tenantManager = Substitute.For<ITenantManager>();
            tenant = Substitute.For<ITenant>();
            tenantManager.GetTenant(tenant.Id).Returns(tenant);

            sut = new SingleTenantContextResolver(tenantManager, tenant.Id);
        }

        [Fact]
        public void Tenant_ReturnsCorrectTenant()
        {
            Assert.Equal(tenant, sut.Tenant);
        }
    }
}
