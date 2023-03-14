using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Metadata
{
    public class ActorNameEventMetadataProvider : IEventMetadataProvider
    {
        private readonly IActorContext actorContext;

        public ActorNameEventMetadataProvider(IActorContext actorContext)
        {
            this.actorContext = actorContext;
        }

        public Task<(string key, string value)[]> GetMetadataAsync(IEventMessage eventMessage)
        {
            return Task.FromResult(new[]
            {
                (BasicEventMetadataNames.ActorName, actorContext.CurrentActorName)
            });
        }
    }
}
