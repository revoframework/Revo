using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Globalization;
using GTRevo.Infrastructure.Globalization.Messages.Database;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Globalization.Messages.Database
{
    public class DbMessageCacheTests
    {
        private readonly IEventBus eventBus;
        private readonly DbMessageCache sut;

        public DbMessageCacheTests()
        {
            eventBus = Substitute.For<IEventBus>();
            sut = new DbMessageCache(eventBus);
        }

        [Fact]
        public void IsInitialized_DefaultsToFalse()
        {
            Assert.False(sut.IsInitialized);
        }

        [Fact]
        public void IsInitialized_TrueWhenLoaded()
        {
            sut.ReplaceMessages(new LocalizationMessage[] { });
            Assert.True(sut.IsInitialized);
        }

        [Fact]
        public void GetLocaleMessages_EmptyForUnknownLocale()
        {
            sut.ReplaceMessages(new LocalizationMessage[] { });
            Assert.Empty(sut.GetLocaleMessages("en-GB"));
        }

        [Fact]
        public void GetLocaleMessages_ReturnsMessagesForLocale()
        {
            sut.ReplaceMessages(new[]
            {
                new LocalizationMessage(Guid.NewGuid(), null, "hello", "hello", new Locale("en-GB"), null),
                new LocalizationMessage(Guid.NewGuid(), null, "hello", "nazdar", new Locale("cs-CZ"), null)
            });

            var actual = sut.GetLocaleMessages("en-GB");
            Assert.Equal(1, actual.Count);
            Assert.Contains(new KeyValuePair<string, string>("hello", "hello"), actual);
        }

        [Fact]
        public void GetLocaleMessages_WithClassNamePrefix()
        {
            sut.ReplaceMessages(new[]
            {
                new LocalizationMessage(Guid.NewGuid(), "myclass", "hello", "hello", new Locale("en-GB"), null)
            });

            var actual = sut.GetLocaleMessages("en-GB");
            Assert.Equal(1, actual.Count);
            Assert.Contains(new KeyValuePair<string, string>("myclass.hello", "hello"), actual);
        }

        [Fact]
        public void ReplaceMessages_PublishesEvent()
        {
            sut.ReplaceMessages(new LocalizationMessage[] {});
            eventBus.Received(1).Publish(Arg.Any<DbMessageCacheReloadedEvent>());
        }
    }
}
