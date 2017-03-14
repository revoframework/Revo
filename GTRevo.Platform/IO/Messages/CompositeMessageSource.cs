using System.Collections.Generic;
using System.Linq;

namespace GTRevo.Platform.IO.Messages
{
    public class CompositeMessageSource : IMessageSource
    {
        private List<IMessageSource> messageSources;

        public CompositeMessageSource(IEnumerable<IMessageSource> messageSources)
        {
            this.messageSources = messageSources.ToList();
        }

        public bool TryGetMessage(string key, out string message)
        {
            for (int i = 0; i < messageSources.Count; i++)
            {
                if (messageSources[i].TryGetMessage(key, out message))
                    return true;
            }

            message = null;
            return false;
        }
    }
}
