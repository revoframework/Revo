using Ninject.Syntax;

namespace GTRevo.Core.Core
{
    public static class NinjectBindingExtensions
    {
        public static INinjectBindingExtension Current { get; set; } = new TransientNinjectBindingExtension();

        public static IBindingNamedWithOrOnSyntax<T> InRequestOrJobScope<T>(this IBindingInSyntax<T> syntax)
        {
            return Current.InRequestOrJobScope(syntax);
        }

        private class TransientNinjectBindingExtension : INinjectBindingExtension
        {
            public IBindingNamedWithOrOnSyntax<T> InRequestOrJobScope<T>(IBindingInSyntax<T> syntax)
            {
                return syntax.InTransientScope();
            }
        }
    }
}
