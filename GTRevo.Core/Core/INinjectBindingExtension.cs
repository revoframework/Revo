using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Syntax;

namespace GTRevo.Core
{
    public interface INinjectBindingExtension
    {
        IBindingNamedWithOrOnSyntax<T> InRequestOrJobScope<T>(IBindingInSyntax<T> syntax);
    }
}
