using System;
using Hangfire;
using Ninject;
using Ninject.Activation.Caching;

namespace Revo.AspNetCore.Core
{
    public class HangfireJobActivator : JobActivator
    {
        private readonly IKernel kernel;
        
        public HangfireJobActivator(IKernel kernel)
        {
            this.kernel = kernel;
        }

        /// <inheritdoc />
        public override object ActivateJob(Type jobType)
        {
            return kernel.Get(jobType);
        }

        [Obsolete("Please implement/use the BeginScope(JobActivatorContext) method instead. Will be removed in 2.0.0.")]
        public override JobActivatorScope BeginScope()
        {
            return new NinjectScope(kernel);
        }

        class NinjectScope : JobActivatorScope
        {
            private readonly IKernel kernel;

            public NinjectScope(IKernel kernel)
            {
                this.kernel = kernel;
            }

            public override object Resolve(Type type)
            {
                return kernel.Get(type);
            }

            public override void DisposeScope()
            {
                kernel.Components.Get<ICache>().Clear(JobActivatorScope.Current);
            }
        }
    }
}
