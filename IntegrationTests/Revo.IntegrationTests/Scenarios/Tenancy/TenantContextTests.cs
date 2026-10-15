using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.IntegrationTests.Fixtures;
using Xunit;

namespace Revo.IntegrationTests.Scenarios.Tenancy
{
    /// <summary>
    /// Tenant context of commands (tenant middleware registered for all commands).
    /// </summary>
    public class TenantContextTests(TestAppFixture fixture) : IClassFixture<TestAppFixture>
    {
        private readonly TestAppHost host = fixture.Host;

        [Fact]
        public async Task ExplicitTenantContext_IsUsedInCommandScope()
        {
            var tenantId = Guid.NewGuid();

            var resolvedTenantId = await host.Client.GetFromJsonAsync<Guid?>($"/tenant?tenantId={tenantId}");

            resolvedTenantId.Should().Be(tenantId);
        }

        [Fact]
        public async Task WithoutExplicitTenantContext_NoTenantIsResolved()
        {
            var resolvedTenantId = await host.Client.GetFromJsonAsync<Guid?>("/tenant");

            resolvedTenantId.Should().BeNull();
        }
    }
}
