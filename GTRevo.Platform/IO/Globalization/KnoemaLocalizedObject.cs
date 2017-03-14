using Knoema.Localization;

namespace GTRevo.Platform.IO.Globalization
{
    public class KnoemaLocalizedObject : ILocalizedObject
    {
        public string Hash { get; set; }
        public int Key { get; set; }
        public int LocaleId { get; set; }
        public string Scope { get; set; }
        public string Text { get; set; }
        public string Translation { get; set; }
    }
}
