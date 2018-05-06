using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Revo.Domain.Tenancy;
using Revo.Infrastructure.Tenancy;
using NSubstitute;
using Xunit;

namespace Revo.Infrastructure.Tests.Tenancy
{
    public class DefaultTenantContextTests
    {
        private readonly DefaultTenantContext sut;
        private readonly ITenantContextResolver tenantContextResolver;
        private readonly ITenant tenant;

        public DefaultTenantContextTests()
        {
            tenantContextResolver = Substitute.For<ITenantContextResolver>();

            tenant = Substitute.For<ITenant>();
            tenant.Id.Returns(Guid.NewGuid());
            tenantContextResolver.ResolveTenant().Returns(tenant);

            sut = new DefaultTenantContext(tenantContextResolver);
        }

        [Fact]
        public void TenantId_ResolvesTenant()
        {
            Assert.Equal(tenant, sut.Tenant);
        }
    }
}
