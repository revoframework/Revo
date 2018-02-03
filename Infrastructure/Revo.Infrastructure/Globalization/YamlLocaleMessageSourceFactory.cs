using System.IO;
using Revo.Infrastructure.Globalization.Messages;
using Revo.Platforms.AspNet.IO.Resources;

namespace Revo.Infrastructure.Globalization
{
    public class YamlLocaleMessageSourceFactory : ILocaleMessageSourceFactory
    {
        private readonly IResourceManager resourceManager;

        public YamlLocaleMessageSourceFactory(string messageResourcePath, string localeCode, int priority,
            IResourceManager resourceManager)
        {
            LocaleCode = localeCode;
            this.resourceManager = resourceManager;
            Priority = priority;
            Load(messageResourcePath);
        }

        public string LocaleCode { get; }
        public int Priority { get; }
        public IMessageSource MessageSource { get; private set; }

        private void Load(string messageResourcePath)
        {
            using (Stream messageStream = resourceManager.CreateReadStream(messageResourcePath))
            {
                YamlMessageSource messageSource = new YamlMessageSource(messageStream);
                MessageSource = messageSource;
            }
        }
    }
}
