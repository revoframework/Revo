using GTRevo.Core.Lifecycle;
using Knoema.Localization;

namespace GTRevo.Platform.IO.Globalization
{
    public class LocalizationAppInitializer : IApplicationStartListener
    {
        private KnoemaLocalizationProvider knoemaLocalizationProvider;

        public LocalizationAppInitializer(KnoemaLocalizationProvider knoemaLocalizationProvider)
        {
            this.knoemaLocalizationProvider = knoemaLocalizationProvider;
        }

        public void OnApplicationStarted()
        {
            LocalizationManager.Provider = knoemaLocalizationProvider;
        }
    }
}
