using System.Collections.Generic;

namespace Revo.Infrastructure.Globalization
{
    public interface ILocaleManager
    {
        IReadOnlyDictionary<string, Locale> Locales { get; }
    }
}