using System;
using System.Collections.Generic;
using Ninject;
using Revo.Core.Core;

namespace Revo.Platforms.AspNet.Core
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

        public IEnumerable<object> GetAll(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }
    }
}
