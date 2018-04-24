using System;
using System.Collections.Generic;

namespace Revo.Core.Core
{
    public interface IServiceLocator
    {
        object Get(Type serviceType);
        T Get<T>();
        IEnumerable<object> GetAll(Type serviceType);
        IEnumerable<T> GetAll<T>();
    }
}
