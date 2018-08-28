using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Configuration;
using Revo.Infrastructure.Events.Async;

namespace Revo.Infrastructure
{
    public class InfrastructureConfigurationSection : IRevoConfigurationSection
    {
        public IAsyncEventPipelineConfiguration AsyncEventPipeline { get; set; } = new AsyncEventPipelineConfiguration();
    }
}
