using System.Collections.Generic;
using System.Linq;
using GTRevo.DataAccess.Entities;
using GTRevo.Platform.IO.Messages;

namespace GTRevo.Infrastructure.Globalization
{
    public class DbMessageSource: IMessageSource
    {
        private readonly Dictionary<string, string> messages = new Dictionary<string, string>();
        private readonly ICrudRepository repository;
        private readonly string culture;

        public DbMessageSource(ICrudRepository repository, string culture)
        {
            this.repository = repository;
            this.culture = culture;
            Load();
        }

        public bool TryGetMessage(string key, out string message)
        {
            if (messages.ContainsKey(key))
            {
                message = messages[key];
                return true;
            }
            message = null;
            return false;
        }

        public IEnumerable<KeyValuePair<string, string>> Messages => messages;
        

        private void Load()
        {
            foreach (var di in repository.FindAll<Dictionary>().Where(d => d.Culture == culture))
            {
                messages.Add($"{di.ClassName}.{di.Key}",di.Translation);
            }
        }
    }
}
