using System.Collections.Immutable;

namespace GTRevo.Infrastructure.Globalization.Messages.Database
{
    public class DbMessageSource : IMessageSource
    {
        private readonly string localeCode;
        private readonly IDbMessageCache dbMessageCache;

        public DbMessageSource(string localeCode, IDbMessageCache dbMessageCache, IDbMessageLoader dbMessageLoader)
        {
            this.localeCode = localeCode;
            this.dbMessageCache = dbMessageCache;

            dbMessageLoader.EnsureLoaded();
        }

        public bool TryGetMessage(string key, out string message)
        {
            return Messages.TryGetValue(key, out message);
        }

        public ImmutableDictionary<string, string> Messages => dbMessageCache.GetLocaleMessages(localeCode);
    }
}
