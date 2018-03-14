using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Revo.Infrastructure.Globalization;

namespace Revo.Extensions.AspNet.Interop.Globalization
{
    public class JsonMessageExportCache
    {
        private readonly LocaleManager localeManager;
        private readonly IMessageRepository messageRepository;
        private Dictionary<string, string> localeDictionaries;

        public JsonMessageExportCache(LocaleManager localeManager,
            IMessageRepository messageRepository)
        {
            this.localeManager = localeManager;
            this.messageRepository = messageRepository;
        }

        public string GetExportedMessages(string localeCode)
        {
            if ((localeDictionaries?.Count ?? 0) == 0)
            {
                Refresh();
            }

            string dictionary;
            if (localeDictionaries.TryGetValue(localeCode, out dictionary))
            {
                return dictionary;
            }

            return new JObject().ToString();
        }

        public void Refresh()
        {
            Dictionary<string, string> locales = new Dictionary<string, string>();
            JsonMessageExporter exporter = new JsonMessageExporter();

            foreach (var locale in localeManager.Locales)
            {
                JObject dictionary = exporter.ExportMessages(messageRepository.GetMessagesForLocale(locale.Value));
                locales.Add(locale.Key, dictionary.ToString());
            }

            localeDictionaries = locales;
        }
    }
}
