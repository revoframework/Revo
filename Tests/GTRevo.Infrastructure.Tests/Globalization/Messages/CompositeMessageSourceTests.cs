using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GTRevo.Infrastructure.Globalization.Messages;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Globalization.Messages
{
    public class CompositeMessageSourceTests
    {
        [Fact]
        public void Messages_HasAllMessages()
        {
            var source1 = Substitute.For<IMessageSource>();
            source1.Messages.Returns(
                ImmutableDictionary.CreateRange<string, string>(new[]
                {
                    new KeyValuePair<string, string>("hello", "ahoj"),
                    new KeyValuePair<string, string>("coffee", "kafe")
                }));

            var source2 = Substitute.For<IMessageSource>();
            source2.Messages.Returns(
                ImmutableDictionary.CreateRange<string, string>(new[]
                {
                    new KeyValuePair<string, string>("coffee", "kava")
                }));

            var sut = new CompositeMessageSource(new[] {source1, source2});

            var expected = new[]
            {
                new KeyValuePair<string, string>("hello", "ahoj"),
                new KeyValuePair<string, string>("coffee", "kava")
            };

            Assert.Equal(expected.Length, sut.Messages.Count);
            Assert.True(sut.Messages.All(x => expected.Contains(x)));
        }
    }
}
