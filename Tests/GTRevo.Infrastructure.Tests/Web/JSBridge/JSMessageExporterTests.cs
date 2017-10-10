using System.Collections.Generic;
using System.Collections.Immutable;
using GTRevo.Infrastructure.Globalization.Messages;
using GTRevo.Infrastructure.Web.JSBridge;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Web.JSBridge
{
    public class JSMessageExporterTests
    {
        private readonly IMessageSource messageSource;

        public JSMessageExporterTests()
        {
            messageSource = Substitute.For<IMessageSource>();
            messageSource.Messages.Returns(
                ImmutableDictionary.CreateRange<string, string>(new[]
                {
                    new KeyValuePair<string, string>("Lorem", "ipsum"),
                    new KeyValuePair<string, string>("Dolor", "sit")
                }));
        }

        [Fact]
        public void ExportMessages_HasCorrectResul()
        {
            var sut = new JsonMessageExporter();

            JObject messages = sut.ExportMessages(messageSource);
            Assert.True(JToken.DeepEquals(JObject.Parse("{\"Lorem\":\"ipsum\", \"Dolor\":\"sit\"}"), messages));
        }
    }
}
