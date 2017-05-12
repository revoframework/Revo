using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Platform.Core
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
