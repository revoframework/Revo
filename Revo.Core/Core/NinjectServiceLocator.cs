using System;
using System.Collections.Generic;
using Ninject;

namespace Revo.Core.Core
{
    public class NinjectServiceLocator(IKernel kernel) : IServiceLocator
    {
        private readonly IKernel kernel = kernel;

        public object Get(Type serviceType) => kernel.Get(serviceType);

        public T Get<T>() => kernel.Get<T>();

        public IEnumerable<object> GetAll(Type serviceType) => kernel.GetAll(serviceType);

        public IEnumerable<T> GetAll<T>() => kernel.GetAll<T>();
    }
}
