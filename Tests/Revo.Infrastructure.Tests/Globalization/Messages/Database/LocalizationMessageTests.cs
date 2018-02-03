using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Infrastructure.Globalization;
using Revo.Infrastructure.Globalization.Messages.Database;
using Xunit;

namespace Revo.Infrastructure.Tests.Globalization.Messages.Database
{
    public class LocalizationMessageTests
    {
        private LocalizationMessage sut;

        public LocalizationMessageTests()
        {
            sut = new LocalizationMessage(Guid.NewGuid(), "cls", "key", "message", new Locale("en-GB"), null);
        }

        [Fact]
        public void SetMessage_PublishesEvent()
        {
            sut.SetMessage("goodbye");
            Assert.Contains(sut.UncommittedEvents.OfType<LocalizationMessageModifiedEvent>(),
                x => x.ClassName == "cls" && x.Key == "key" && x.LocaleCode == "en-GB"
                && x.Message == "message" && x.TenantId == null);
        }

        [Fact]
        public void Delete_PublishesEvent()
        {
            sut.Delete();
            Assert.Contains(sut.UncommittedEvents.OfType<LocalizationMessageDeletedEvent>(),
                x => true);
        }

        [Fact]
        public void Delete_MarksDeleted()
        {
            sut.Delete();
            Assert.True(sut.IsDeleted);
        }
    }
}
