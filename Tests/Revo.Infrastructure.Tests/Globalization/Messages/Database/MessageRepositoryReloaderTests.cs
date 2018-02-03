using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Globalization.Messages.Database;
using Xunit;
using Revo.Infrastructure.Globalization;
using Revo.Testing.Infrastructure;
using NSubstitute;

namespace Revo.Infrastructure.Tests.Globalization.Messages.Database
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
