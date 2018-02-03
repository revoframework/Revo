using Knoema.Localization;
using Revo.Core.Core.Lifecycle;

namespace Revo.Infrastructure.Globalization
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
