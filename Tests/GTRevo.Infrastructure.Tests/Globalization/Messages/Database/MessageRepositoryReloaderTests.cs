using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Globalization.Messages.Database;
using Xunit;
using GTRevo.Infrastructure.Globalization;
using NSubstitute;

namespace GTRevo.Infrastructure.Tests.Globalization.Messages.Database
{
    public class MessageRepositoryReloaderTests
    {
        private readonly IMessageRepository messageRepository;
        private readonly MessageRepositoryReloader sut;

        public MessageRepositoryReloaderTests()
        {
            messageRepository = Substitute.For<IMessageRepository>();
            sut = new MessageRepositoryReloader(messageRepository);
        }

        [Fact]
        public void Handle_DbMessageCacheReloadedEvent_ReloadsRepository()
        {
            sut.Handle(new DbMessageCacheReloadedEvent());
            messageRepository.Received(1).Reload();
        }
    }
}
