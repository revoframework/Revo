using System.Collections.Generic;

namespace GTRevo.Platform.IO.Messages
{
    public interface IMessageSource
    {
        IEnumerable<KeyValuePair<string, string>> Messages { get; }
        bool TryGetMessage(string key, out string message);
    }
}
