using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Events.Metadata
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
