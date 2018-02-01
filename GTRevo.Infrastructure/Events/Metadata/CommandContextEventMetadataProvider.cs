using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Commands;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Events.Metadata
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
