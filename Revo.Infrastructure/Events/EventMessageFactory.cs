using System;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Infrastructure.Events.Metadata;

namespace Revo.Infrastructure.Events
{
    public class EventMessageFactory(IEventMetadataProvider[] metadataProviders) : IEventMessageFactory
    {
        public async Task<IEventMessageDraft> CreateMessageAsync(IEvent @event)
        {
            Type messageType = typeof(EventMessageDraft<>).MakeGenericType(@event.GetType());
            IEventMessageDraft messageDraft = (IEventMessageDraft) messageType.GetConstructor(new[] {@event.GetType()}).Invoke(new[] {@event});
            
            foreach (var metadataProvider in metadataProviders)
            {
                var metadata = await metadataProvider.GetMetadataAsync(messageDraft);
                foreach (var pair in metadata)
                {
                    messageDraft.SetMetadata(pair.key, pair.value);
                }
            }

            return messageDraft;
        }
    }
}
