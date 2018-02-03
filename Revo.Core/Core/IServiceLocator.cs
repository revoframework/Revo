using System;
using System.Collections.Generic;

namespace Revo.Core.Core
{
    public interface IServiceLocator
    {
        object Get(Type serviceType);
        IEnumerable<object> GetAll(Type serviceType);
    }
}
