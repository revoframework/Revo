using System;
using Ninject.Infrastructure;
using Ninject.Syntax;

namespace Revo.Core.Core
{
    /// <summary>
    /// Ninject extension methods for framework-specific binding styles.
    /// </summary>
    public static class NinjectBindingExtensions
    {
        public static INinjectBindingExtension Current { get; set; } = new TaskNinjectBindingExtension();

        /// <summary>
        /// Registers a service in a scope of the (Web API) request.
        /// If there is no active request (e.g. run from a job), might fall back to using a different suitable scope
        /// (e.g. Task with a context, <see cref="InTaskScope{T}"/>).
        /// </summary>
        public static IBindingNamedWithOrOnSyntax<T> InRequestScope<T>(this IBindingInSyntax<T> syntax)
        {
            return Current.InRequestScope(syntax);
        }

        /// <summary>
        /// Registers a service in a scope of a closest parent Task that was started with
        /// a context via TaskFactoryExtensions.StartNewWithContext which is flown across async calls.
        /// If there is no parent Task with a context, might fall back to using a different suitable scope (e.g. request or thread).
        /// </summary>
        public static IBindingNamedWithOrOnSyntax<T> InTaskScope<T>(this IBindingInSyntax<T> syntax)
        {
            return Current.InTaskScope(syntax);
        }

        [Obsolete("Use InTaskScope or InRequestScope instead.")]
        public static IBindingNamedWithOrOnSyntax<T> InRequestOrJobScope<T>(this IBindingInSyntax<T> syntax)
        {
            return Current.InTaskScope(syntax);
        }

        private class TaskNinjectBindingExtension : INinjectBindingExtension
        {
            public IBindingNamedWithOrOnSyntax<T> InRequestScope<T>(IBindingInSyntax<T> syntax)
            {
                return InTaskScope(syntax);
            }

            public IBindingNamedWithOrOnSyntax<T> InTaskScope<T>(IBindingInSyntax<T> syntax)
            {
                return syntax
                    .InScope(context => TaskContext.Current ?? StandardScopeCallbacks.Thread(context));
            }
        }
    }
}
