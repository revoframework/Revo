using System.Threading;
using Ninject.Infrastructure;
using Ninject.Syntax;

namespace Revo.Core.Core
{
    public static class NinjectBindingExtensions
    {
        public static INinjectBindingExtension Current { get; set; } = new TaskNinjectBindingExtension();

        public static IBindingNamedWithOrOnSyntax<T> InRequestOrJobScope<T>(this IBindingInSyntax<T> syntax)
        {
            return Current.InRequestOrJobScope(syntax);
        }

        private class TaskNinjectBindingExtension : INinjectBindingExtension
        {
            public IBindingNamedWithOrOnSyntax<T> InRequestOrJobScope<T>(IBindingInSyntax<T> syntax)
            {
                return syntax
                    .InScope(context => TaskContext.Current ?? StandardScopeCallbacks.Thread(context));
            }
        }
    }
}
