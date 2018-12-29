using Revo.Core.Configuration;
using Revo.Infrastructure.Events.Async;

namespace Revo.Infrastructure
{
    public class InfrastructureConfigurationSection : IRevoConfigurationSection
    {
        public IAsyncEventPipelineConfiguration AsyncEventPipeline { get; set; } = new AsyncEventPipelineConfiguration();
        public bool EnableSagas { get; set; } = true;
    }
}
