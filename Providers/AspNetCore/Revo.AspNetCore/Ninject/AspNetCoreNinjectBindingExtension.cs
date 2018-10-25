using Ninject.Infrastructure;
using Ninject.Syntax;
using Revo.Core.Core;

namespace Revo.AspNetCore.Ninject
{
    public class AspNetCoreNinjectBindingExtension : INinjectBindingExtension
    {
        public IBindingNamedWithOrOnSyntax<T> InRequestScope<T>(IBindingInSyntax<T> syntax)
        {
            return syntax
                .InScope(context =>
                   RevoStartup.RequestScope(context)
                    ?? (object)TaskContext.Current
                    ?? StandardScopeCallbacks.Thread(context));
        }

        public IBindingNamedWithOrOnSyntax<T> InTaskScope<T>(IBindingInSyntax<T> syntax)
        {
            return syntax
                .InScope(context =>
                    (object) TaskContext.Current
                    ?? RevoStartup.RequestScope(context)
                    ?? StandardScopeCallbacks.Thread(context));
        }
    }
}
