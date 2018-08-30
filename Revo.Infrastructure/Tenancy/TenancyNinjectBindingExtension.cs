using Ninject;
using Ninject.Syntax;

namespace Revo.Infrastructure.Tenancy
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
