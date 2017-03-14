using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GTRevo.Platform.IO.Globalization
{
    public class LocaleManager
    {
        private Dictionary<string, Locale> locales = new Dictionary<string, Locale>();
        private ReadOnlyDictionary<string, Locale> readOnlyLocales;

        public IReadOnlyDictionary<string, Locale> Locales
        {
            get
            {
                return readOnlyLocales ?? (readOnlyLocales = new ReadOnlyDictionary<string, Locale>(locales));
            }
        }

        public void RegisterLocale(Locale locale)
        {
            if (locales.ContainsKey(locale.Code))
            {
                throw new ArgumentException("Locale already registered");
            }

            locales.Add(locale.Code, locale);
        }
    }
}
