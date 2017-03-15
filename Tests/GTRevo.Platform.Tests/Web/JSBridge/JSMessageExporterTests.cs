using System.Collections.Generic;
using GTRevo.Platform.IO.Messages;
using GTRevo.Platform.Web.JSBridge;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace GTRevo.Platform.Tests.Web.JSBridge
{
    public class JSMessageExporterTests
    {
        private readonly IMessageSource messageSource;

        public JSMessageExporterTests()
        {
            messageSource = Substitute.For<IMessageSource>();
            messageSource.Messages.Returns(
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Lorem", "ipsum"),
                    new KeyValuePair<string, string>("Dolor", "sit")
                });
        }

        [Fact]
        public void TestExportMessages()
        {
            var sut = new JSMessageExporter();

            JObject messages = sut.ExportMessages(messageSource);
            Assert.Equal(JObject.Parse("{\"Lorem\":\"ipsum\", \"Dolor\":\"sit\"}"), messages);
        }
    }
}
