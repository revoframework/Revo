using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Core.Collections
{
    public static class Extensions
    {
        public static string ToSeparatedString(this IEnumerable source, char separator)
        {
            var isFirst = true;
            var sb = new StringBuilder();
            foreach (var item in source)
            {
                if (!isFirst)
                    sb.Append(separator);
                sb.Append(item);
                isFirst = false;
            }
            return sb.ToString();
        }
    }
}
