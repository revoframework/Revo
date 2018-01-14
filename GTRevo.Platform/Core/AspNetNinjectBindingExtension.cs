using System.Linq;
using GTRevo.Core;
using GTRevo.Core.Core;
using Hangfire;
using Ninject.Syntax;
using Ninject.Web.Common;

namespace GTRevo.Platform.Core
{
    public class AspNetNinjectBindingExtension : INinjectBindingExtension
    {
        public IBindingNamedWithOrOnSyntax<T> InRequestOrJobScope<T>(IBindingInSyntax<T> syntax)
        {
            return syntax
                .InScope(context => context.Kernel.Components.GetAll<INinjectHttpApplicationPlugin>()
                    .Select(c => c.GetRequestScope(context)).FirstOrDefault(s => s != null)
                    ?? (object) JobActivatorScope.Current
                    ?? (object) TaskContext.Current);
        }
    }
}
