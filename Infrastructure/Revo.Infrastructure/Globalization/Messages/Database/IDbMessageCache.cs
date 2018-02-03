using System.Collections.Generic;
using System.Collections.Immutable;

namespace Revo.Infrastructure.Globalization.Messages.Database
{
    public interface IDbMessageCache
    {
        bool IsInitialized { get; }

        ImmutableDictionary<string, string> GetLocaleMessages(string localeCode);
        void ReplaceMessages(IEnumerable<LocalizationMessage> data);
    }
}