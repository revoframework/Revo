using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using Ninject;

namespace GTRevo.Platform.Core
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
