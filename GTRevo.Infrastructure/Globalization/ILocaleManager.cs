using System.Collections.Generic;

namespace GTRevo.Infrastructure.Globalization
{
    public interface ILocaleManager
    {
        IReadOnlyDictionary<string, Locale> Locales { get; }
    }
}