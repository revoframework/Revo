using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.Globalization
{
    public class LocaleLoader : IApplicationStartListener
    {
        private readonly LocaleManager localeManager;
        private readonly LocaleRegistration[] localeRegistrations;

        public LocaleLoader(LocaleManager localeManager,
            LocaleRegistration[] localeRegistrations)
        {
            this.localeManager = localeManager;
            this.localeRegistrations = localeRegistrations;
        }

        public void OnApplicationStarted()
        {
            Load();
        }

        private void Load()
        {
            foreach (LocaleRegistration localeReg in localeRegistrations)
            {
                if (localeManager.Locales.ContainsKey(localeReg.Code))
                {
                    continue;
                }

                Locale locale = new Locale(localeReg.Code);
                localeManager.RegisterLocale(locale);
            }
        }
    }
}
