using Ninject.Syntax;
using Ninject.Web.Common;

namespace GTRevo.Platform.Core
{
    public static class NinjectBindingExtension
    {
        public static IBindingNamedWithOrOnSyntax<T> InRequestOrJobScope<T>(this IBindingInSyntax<T> syntax)
        {
            return syntax.InRequestScope();

            //TODO: once we implement Hangfire, use this version 
            /*return syntax
                .InNamedOrBackgroundJobScope(context => context.Kernel.Components.GetAll<INinjectHttpApplicationPlugin>()
                    .Select(c => c.GetRequestScope(context)).FirstOrDefault(s => s != null));*/
        }
    }
}
