using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTRevo.Core.Core.Lifecycle;
using GTRevo.Platform.IO.Messages;
using GTRevo.Platform.IO.Resources;

namespace GTRevo.Platform.IO.Globalization
{
    public class LocaleLoader : IApplicationStartListener
    {
        private LocaleManager localeManager;
        private IResourceManager resourceManager;
        private LocaleRegistration[] localeRegistrations;
        private LocaleMessageResourceRegistration[] localeMessageResourceRegistrations;
        private IMessageRepository messageRepository;

        public LocaleLoader(LocaleManager localeManager,
            IResourceManager resourceManager,
            LocaleMessageResourceRegistration[] localeMessageResourceRegistrations,
            LocaleRegistration[] localeRegistrations,
            IMessageRepository messageRepository)
        {
            this.localeManager = localeManager;
            this.resourceManager = resourceManager;
            this.localeMessageResourceRegistrations = localeMessageResourceRegistrations;
            this.localeRegistrations = localeRegistrations;
            this.messageRepository = messageRepository;
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

                List<IMessageSource> messageSources = new List<IMessageSource>();

                foreach (LocaleMessageResourceRegistration messageResourceReg
                    in localeMessageResourceRegistrations.Where(x => x.LocaleCode == locale.Code))
                {
                    if (messageResourceReg.Source != null)
                        messageSources.Add(messageResourceReg.Source);
                    else
                        using (Stream messageStream = resourceManager.CreateReadStream(messageResourceReg.MessageResourcePath))
                        {
                            YamlMessageSource messageSource = new YamlMessageSource(messageStream);
                            messageSources.Add(messageSource);
                        }
                }

                IMessageSource compositeMessageSource = new CompositeMessageSource(messageSources);
                messageRepository.SetMessagesForLocale(locale, compositeMessageSource);
            }
        }
    }
}
