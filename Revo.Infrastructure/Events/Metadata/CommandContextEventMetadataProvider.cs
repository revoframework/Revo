using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Metadata
{
    public class CommandContextEventMetadataProvider : IEventMetadataProvider
    {
        private readonly ICommandContext commandContext;

        public CommandContextEventMetadataProvider(ICommandContext commandContext)
        {
            this.commandContext = commandContext;
        }

        public Task<(string key, string value)[]> GetMetadataAsync(IEventMessage eventMessage)
        {
            if (commandContext.CurrentCommand != null)
            {
                return Task.FromResult(new[]
                {
                    //(BasicEventMetadataNames.CommandId, commandContext.CurrentCommand.Id),
                    (BasicEventMetadataNames.CommandTypeId, commandContext.CurrentCommand.GetType().FullName)
                });
            }
            else
            {
                return Task.FromResult(new (string key, string value)[] {});
            }
        }
    }
}
