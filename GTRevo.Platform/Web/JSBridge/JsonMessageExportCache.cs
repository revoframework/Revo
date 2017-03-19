using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Platform.IO.Globalization;
using Newtonsoft.Json.Linq;

namespace GTRevo.Platform.Web.JSBridge
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

            Initialize();
        }

        public string GetExportedMessages(string localeCode)
        {
            string dictionary;
            if (localeDictionaries.TryGetValue(localeCode, out dictionary))
            {
                return dictionary;
            }

            return new JObject().ToString();
        }

        private void Initialize()
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
