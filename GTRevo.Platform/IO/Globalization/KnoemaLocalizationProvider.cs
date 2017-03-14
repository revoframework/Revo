using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Knoema.Localization;

namespace GTRevo.Platform.IO.Globalization
{
    public class KnoemaLocalizationProvider : ILocalizationProvider
    {
        private LocaleManager localeManager;
        private IMessageRepository messageRepository;

        public KnoemaLocalizationProvider(IMessageRepository messageRepository,
            LocaleManager localeManager)
        {
            this.messageRepository = messageRepository;
            this.localeManager = localeManager;
        }

        public ILocalizedObject Create(CultureInfo culture, string scope, string text)
        {
            throw new NotImplementedException();
        }

        public void Delete(params ILocalizedObject[] list)
        {
            throw new NotImplementedException();
        }

        public ILocalizedObject Get(int key)
        {
            throw new NotImplementedException();
        }

        public ILocalizedObject Get(CultureInfo culture, string scope, string text)
        {
            Locale locale = localeManager.Locales[culture.Name]; /* TODO: lookup by CultureInfo? */
            var messages = messageRepository.GetMessagesForLocale(locale);

            string message;
            messages.TryGetMessage(text, out message);

            return new KnoemaLocalizedObject()
            {
                Key = text.GetHashCode(),
                Text = text,
                Scope = "",
                LocaleId = culture.LCID,
                Translation = message
            };
        }

        public IEnumerable<ILocalizedObject> GetAll(CultureInfo culture)
        {
            return new List<ILocalizedObject>();
        }

        public IEnumerable<CultureInfo> GetCultures()
        {
            return localeManager.Locales.Select(x => x.Value.CultureInfo);
        }

        public string GetRoot()
        {
            return null;
        }

        public void Save(params ILocalizedObject[] list)
        {
            throw new NotImplementedException();
        }
    }
}
