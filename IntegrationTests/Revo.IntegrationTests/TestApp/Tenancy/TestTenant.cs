using System;
using Revo.Core.Tenancy;

namespace Revo.IntegrationTests.TestApp.Tenancy
{
    public record TestTenant(Guid Id, string Name) : ITenant;
}
