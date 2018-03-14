using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Revo.Extensions.AspNet.Interop.Globalization;
using Xunit;
using Revo.Infrastructure.Globalization.Messages;

namespace Revo.Extensions.AspNet.Interop.Tests.Globalization
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
        public void ExportMessages_HasCorrectResult()
        {
            var sut = new JsonMessageExporter();

            JObject messages = sut.ExportMessages(messageSource);
            Assert.True(JToken.DeepEquals(JObject.Parse("{\"Lorem\":\"ipsum\", \"Dolor\":\"sit\"}"), messages));
        }
    }
}
