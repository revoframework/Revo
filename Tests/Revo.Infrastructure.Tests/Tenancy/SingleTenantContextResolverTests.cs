using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Domain.Tenancy;
using Revo.Infrastructure.Tenancy;
using NSubstitute;
using Xunit;

namespace Revo.Infrastructure.Tests.Tenancy
{
    public class SingleTenantContextResolverTests
    {
        private readonly SingleTenantContextResolver sut;
        private readonly ITenantProvider tenantProvider;
        private readonly ITenant tenant;

        public SingleTenantContextResolverTests()
        {
            tenantProvider = Substitute.For<ITenantProvider>();
            tenant = Substitute.For<ITenant>();
            tenantProvider.GetTenant(tenant.Id).Returns(tenant);

            sut = new SingleTenantContextResolver(tenantProvider, tenant.Id);
        }

        [Fact]
        public void Tenant_ReturnsCorrectTenant()
        {
            Assert.Equal(tenant, sut.Tenant);
        }
    }
}
