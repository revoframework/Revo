using Ninject.Syntax;

namespace Revo.Core.Core
{
    public interface INinjectBindingExtension
    {
        IBindingNamedWithOrOnSyntax<T> InRequestOrJobScope<T>(IBindingInSyntax<T> syntax);
    }
}
