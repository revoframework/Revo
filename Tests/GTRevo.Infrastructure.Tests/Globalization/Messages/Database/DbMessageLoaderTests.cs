using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Events;
using GTRevo.Infrastructure.Globalization;
using GTRevo.Infrastructure.Globalization.Messages.Database;
using GTRevo.Testing.DataAccess.EF6;
using GTRevo.Testing.Infrastructure;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Globalization.Messages.Database
{
    public class DbMessageLoaderTests
    {
        private readonly FakeCrudRepository fakeCrudRepository;
        private readonly IDbMessageCache dbMessageCache;
        private readonly LocalizationMessage[] messages;
        private readonly DbMessageLoader sut;

        public DbMessageLoaderTests()
        {
            fakeCrudRepository = new FakeCrudRepository();
            dbMessageCache = Substitute.For<IDbMessageCache>();

            messages = new[]
            {
                new LocalizationMessage(Guid.NewGuid(), null, "hello", "ahoj", new Locale("cs-CZ"), null),
                new LocalizationMessage(Guid.NewGuid(), null, "coffee", "kafe", new Locale("cs-CZ"), null)
            };

            fakeCrudRepository.AttachRange(messages);

            sut = new DbMessageLoader(dbMessageCache, fakeCrudRepository);
        }

        [Fact]
        public void EnsureLoaded_LoadsIfNotInitialized()
        {
            dbMessageCache.IsInitialized.Returns(false);
            sut.EnsureLoaded();

            dbMessageCache.Received(1).ReplaceMessages(Arg.Is<IEnumerable<LocalizationMessage>>(x =>
                x.Count() == messages.Length
                && x.All(y => messages.Contains(y))));
        }

        [Fact]
        public void EnsureLoaded_DoesntLoadIfInitialized()
        {
            dbMessageCache.IsInitialized.Returns(true);
            sut.EnsureLoaded();

            dbMessageCache.ReceivedWithAnyArgs(0).ReplaceMessages(null);
        }

        [Fact]
        public void Reload_ReloadsCache()
        {
            sut.Reload();

            dbMessageCache.Received(1).ReplaceMessages(Arg.Is<IEnumerable<LocalizationMessage>>(x =>
                x.Count() == messages.Length
                && x.All(y => messages.Contains(y))));
        }

        [Fact]
        public async Task Handle_LocalizationMessageModifiedEvent_Reloads()
        {
            await sut.HandleAsync(
                new LocalizationMessageModifiedEvent(null, "hello", "is it you", "cs-CZ", null).ToMessageDraft(), CancellationToken.None);

            dbMessageCache.Received(1).ReplaceMessages(Arg.Is<IEnumerable<LocalizationMessage>>(x =>
                x.Count() == messages.Length
                && x.All(y => messages.Contains(y))));
        }

        [Fact]
        public async Task Handle_LocalizationMessageDeletedEvent_Reloads()
        {
            await sut.HandleAsync(new EventMessageDraft<LocalizationMessageDeletedEvent>(new LocalizationMessageDeletedEvent()),
                CancellationToken.None);

            dbMessageCache.Received(1).ReplaceMessages(Arg.Is<IEnumerable<LocalizationMessage>>(x =>
                x.Count() == messages.Length
                && x.All(y => messages.Contains(y))));
        }
    }
}
