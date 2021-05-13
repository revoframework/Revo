using System;
using System.Collections.Generic;
using Ninject;

namespace Revo.Core.Core
{
    public class NinjectServiceLocator : IServiceLocator
    {
        private readonly IKernel kernel;

        public NinjectServiceLocator(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public object Get(Type serviceType)
        {
            return kernel.Get(serviceType);
        }

        public T Get<T>()
        {
            return kernel.Get<T>();
        }

        public IEnumerable<object> GetAll(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }

        public IEnumerable<T> GetAll<T>()
        {
            return kernel.GetAll<T>();
        }
    }
}
