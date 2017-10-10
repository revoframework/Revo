using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Syntax;
using Ninject;

namespace GTRevo.Infrastructure.Tenancy
{
    public static class TenancyNinjectBindingExtension
    {
        private static readonly object NoTenantContext = new object();

        public static IBindingNamedWithOrOnSyntax<T> InTenantSingletonScope<T>(this IBindingInSyntax<T> syntax)
        {
            return syntax.InScope(context => context.Kernel.Get<ITenantContext>()?.Tenant ?? NoTenantContext);
        }
    }
}
