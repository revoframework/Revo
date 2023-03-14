using System;
using System.Threading.Tasks;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Metadata
{
    public class MachineNameEventMetadataProvider : IEventMetadataProvider
    {
        public Task<(string key, string value)[]> GetMetadataAsync(IEventMessage eventMessage)
        {
            return Task.FromResult(new[]
            {
                (BasicEventMetadataNames.MachineName, Environment.MachineName)
            });
        }
    }
}
