using System;

namespace GTRevo.Core.Core
{
    public static class StringExtensions
    {
        public static int IndexOfI(this string haystack, string needle)
        {
            return haystack.IndexOf(needle, StringComparison.Ordinal);
        }

        public static int IndexOfI(this string haystack, string needle, int startIndex)
        {
            return haystack.IndexOf(needle, startIndex, StringComparison.Ordinal);
        }

        public static int IndexOfI(this string haystack, string needle, int startIndex, int count)
        {
            return haystack.IndexOf(needle, startIndex, count, StringComparison.Ordinal);
        }

        public static int LastIndexOfI(this string haystack, string needle)
        {
            return haystack.LastIndexOf(needle, StringComparison.Ordinal);
        }
    }
}
