using System;
using System.Linq;
using Revo.Core.Configuration;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Sagas;

namespace Revo.Infrastructure
{
    public static class InfrastructureConfigurationExtensions
    {
        public static IRevoConfiguration ConfigureInfrastructure(this IRevoConfiguration configuration,
            AsyncEventPipelineConfiguration asyncEventPipelineConfiguration = null,
            Action<InfrastructureConfigurationSection> advancedAction = null)
        {
            configuration.ConfigureCore();

            var section = configuration.GetSection<InfrastructureConfigurationSection>();
            section.AsyncEventPipeline = asyncEventPipelineConfiguration ?? section.AsyncEventPipeline;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (!c.Kernel.GetBindings(typeof(IAsyncEventPipelineConfiguration)).Any())
                {
                    c.Kernel.Bind<IAsyncEventPipelineConfiguration>().ToConstant(section.AsyncEventPipeline);
                }

                if (section.EnableSagas)
                {
                    c.LoadModule<SagasModule>();
                }
            });

            return configuration;
        }
    }
}
