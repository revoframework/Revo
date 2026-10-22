using System;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Infrastructure.Tenancy;
using Revo.IntegrationTests.TestApp.Messages.Queries;

namespace Revo.IntegrationTests.TestApp.Handlers
{
    public class TenantQueryHandler(ITenantContext tenantContext) :
        IQueryHandler<GetCurrentTenantIdQuery, Guid?>
    {
        public Task<Guid?> HandleAsync(GetCurrentTenantIdQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(tenantContext.Tenant?.Id);
        }
    }
}
