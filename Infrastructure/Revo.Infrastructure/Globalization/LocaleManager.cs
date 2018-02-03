using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Revo.Infrastructure.Globalization
{
    public class LocaleManager : ILocaleManager
    {
        private readonly Dictionary<string, Locale> locales = new Dictionary<string, Locale>();
        private ReadOnlyDictionary<string, Locale> readOnlyLocales;

        public IReadOnlyDictionary<string, Locale> Locales => readOnlyLocales ?? (readOnlyLocales = new ReadOnlyDictionary<string, Locale>(locales));

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
