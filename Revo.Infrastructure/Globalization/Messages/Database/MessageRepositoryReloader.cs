using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Events;

namespace Revo.Infrastructure.Globalization.Messages.Database
{
    public class MessageRepositoryReloader : IEventListener<DbMessageCacheReloadedEvent>
    {
        private readonly IMessageRepository messageRepository;

        public MessageRepositoryReloader(IMessageRepository messageRepository)
        {
            this.messageRepository = messageRepository;
        }

        public Task HandleAsync(IEventMessage<DbMessageCacheReloadedEvent> message, CancellationToken cancellationToken)
        {
            return messageRepository.ReloadAsync();
        }
    }
}
