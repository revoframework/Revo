using Ninject.Syntax;

namespace Revo.Core.Core
{
    public interface INinjectBindingExtension
    {
        IBindingNamedWithOrOnSyntax<T> InTaskScope<T>(IBindingInSyntax<T> syntax);
        IBindingNamedWithOrOnSyntax<T> InRequestScope<T>(IBindingInSyntax<T> syntax);
    }
}
