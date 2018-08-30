using System.Linq;
using Ninject.Infrastructure;
using Ninject.Syntax;
using Ninject.Web.Common;
using Revo.Core.Core;

namespace Revo.AspNet.Core
{
    public class AspNetNinjectBindingExtension : INinjectBindingExtension
    {
        public IBindingNamedWithOrOnSyntax<T> InRequestOrJobScope<T>(IBindingInSyntax<T> syntax)
        {
            return syntax
                .InScope(context =>
                    (object) TaskContext.Current
                    ?? context.Kernel.Components.GetAll<INinjectHttpApplicationPlugin>()
                        .Select(c => c.GetRequestScope(context)).FirstOrDefault(s => s != null)
                    ?? StandardScopeCallbacks.Thread(context));
        }
    }
}
