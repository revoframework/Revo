using System;
using Revo.Core.Configuration;
using Revo.Infrastructure.Events.Async;

namespace Revo.Infrastructure
{
    public static class InfrastructureConfigurationExtensions
    {
        public static IRevoConfiguration ConfigureInfrastructure(this IRevoConfiguration configuration,
            AsyncEventPipelineConfiguration asyncEventPipelineConfiguration = null,
            Action<InfrastructureConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<InfrastructureConfigurationSection>();
            section.AsyncEventPipeline = asyncEventPipelineConfiguration ?? section.AsyncEventPipeline;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                c.Kernel.Bind<IAsyncEventPipelineConfiguration>().ToConstant(section.AsyncEventPipeline);
            });

            return configuration;
        }
    }
}
