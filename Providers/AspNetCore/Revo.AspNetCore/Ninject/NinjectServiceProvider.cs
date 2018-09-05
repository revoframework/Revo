using System;
using System.Collections.Generic;
using System.Text;
using Revo.Core.Core;

namespace Revo.AspNetCore.Ninject
{
    public class NinjectServiceProvider : IServiceProvider
    {
        private readonly IServiceLocator serviceLocator;

        public NinjectServiceProvider(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public object GetService(Type serviceType)
        {
            return serviceLocator.Get(serviceType);
        }
    }
}
