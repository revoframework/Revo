using GTRevo.Core.Core.Lifecycle;
using Knoema.Localization;

namespace GTRevo.Infrastructure.Globalization
{
    public class LocalizationAppInitializer : IApplicationStartListener
    {
        private readonly KnoemaLocalizationProvider knoemaLocalizationProvider;

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
