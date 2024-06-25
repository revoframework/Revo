using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Revo.Core.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public sealed class TenantContextOverride(ITenant tenant) : IDisposable
    {
        private static readonly AsyncLocal<TenantContextOverride[]> CurrentLocal = new AsyncLocal<TenantContextOverride[]>();
        private bool isDisposed;

        public static TenantContextOverride Current => CurrentLocal.Value?.LastOrDefault();

        public ITenant Tenant { get; } = tenant;

        public static TenantContextOverride Push(ITenant tenant)
        {
            var newTenants = CurrentLocal.Value;
            var tenantContext = new TenantContextOverride(tenant);
            newTenants = newTenants != null ? newTenants.Append(tenantContext).ToArray() : new[] { tenantContext };

            CurrentLocal.Value = newTenants;
            return tenantContext;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                Debug.Assert(CurrentLocal.Value?.LastOrDefault() == this);

                CurrentLocal.Value = CurrentLocal.Value.Length == 1
                    ? null
                    : CurrentLocal.Value.Take(CurrentLocal.Value.Length - 1).ToArray();

                isDisposed = true;
            }
        }
    }
}