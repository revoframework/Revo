using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Globalization.Messages.Database
{
    public class MessageRepositoryReloader : IEventListener<DbMessageCacheReloadedEvent>
    {
        private readonly IMessageRepository messageRepository;

        public MessageRepositoryReloader(IMessageRepository messageRepository)
        {
            this.messageRepository = messageRepository;
        }

        public Task Handle(DbMessageCacheReloadedEvent notification)
        {
            messageRepository.Reload();
            return Task.FromResult(0);
        }
    }
}
