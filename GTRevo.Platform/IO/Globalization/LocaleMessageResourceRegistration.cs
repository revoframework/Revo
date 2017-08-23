using GTRevo.Platform.IO.Messages;

namespace GTRevo.Platform.IO.Globalization
{
    public class LocaleMessageResourceRegistration
    {
        public string MessageResourcePath { get; set; }
        public IMessageSource Source { get; set; }
        public string LocaleCode { get; set; }
    }
}
