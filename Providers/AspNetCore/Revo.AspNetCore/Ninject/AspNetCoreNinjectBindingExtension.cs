using Ninject.Infrastructure;
using Ninject.Syntax;
using Revo.Core.Core;

namespace Revo.AspNetCore.Ninject
{
    public class AspNetCoreNinjectBindingExtension : INinjectBindingExtension
    {
        public IBindingNamedWithOrOnSyntax<T> InRequestOrJobScope<T>(IBindingInSyntax<T> syntax)
        {
            return syntax
                .InScope(context =>
                    (object) TaskContext.Current
                    ?? RevoStartup.RequestScope(context)
                    ?? StandardScopeCallbacks.Thread(context));
        }
    }
}
