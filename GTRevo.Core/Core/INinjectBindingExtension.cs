using Ninject.Syntax;

namespace GTRevo.Core.Core
{
    public interface INinjectBindingExtension
    {
        IBindingNamedWithOrOnSyntax<T> InRequestOrJobScope<T>(IBindingInSyntax<T> syntax);
    }
}
