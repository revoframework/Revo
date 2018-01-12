using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Events.Metadata;

namespace GTRevo.Infrastructure.Events
{
    public class EventMessageFactory : IEventMessageFactory
    {
        private readonly IEventMetadataProvider[] metadataProviders;

        public EventMessageFactory(IEventMetadataProvider[] metadataProviders)
        {
            this.metadataProviders = metadataProviders;
        }

        public async Task<IEventMessageDraft> CreateMessageAsync(IEvent @event)
        {
            Type messageType = typeof(EventMessageDraft<>).MakeGenericType(@event.GetType());
            IEventMessageDraft messageDraft = (IEventMessageDraft) messageType.GetConstructor(new[] {@event.GetType()}).Invoke(new[] {@event});
            
            foreach (var metadataProvider in metadataProviders)
            {
                var metadata = await metadataProvider.GetMetadataAsync(messageDraft);
                foreach (var pair in metadata)
                {
                    if (!messageDraft.Metadata.ContainsKey(pair.key))
                    {
                        messageDraft.AddMetadata(pair.key, pair.value);
                    }
                }
            }

            return messageDraft;
        }
    }
}
