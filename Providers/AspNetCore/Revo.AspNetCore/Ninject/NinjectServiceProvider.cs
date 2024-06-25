using System;
using Revo.Core.Core;

namespace Revo.AspNetCore.Ninject
{
    public class NinjectServiceProvider(IServiceLocator serviceLocator) : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            return serviceLocator.Get(serviceType);
        }
    }
}
