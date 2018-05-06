using Knoema.Localization;
using Revo.Core.Lifecycle;

namespace Revo.Platforms.AspNet.Globalization
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
