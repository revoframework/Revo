using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Events;
using GTRevo.Infrastructure.Globalization.Messages.Database;
using Xunit;
using GTRevo.Infrastructure.Globalization;
using GTRevo.Testing.Infrastructure;
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
        public async Task Handle_DbMessageCacheReloadedEvent_ReloadsRepository()
        {
            await sut.HandleAsync(new DbMessageCacheReloadedEvent().ToMessageDraft(),
                CancellationToken.None);
            messageRepository.Received(1).ReloadAsync();
        }
    }
}
