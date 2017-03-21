using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Platform.Web
{
    public static class HttpMethodHelpers
    {
        public static bool IsSafe(this HttpMethod httpMethod)
        {
            return httpMethod != HttpMethod.Post
                   && httpMethod != HttpMethod.Delete
                   && httpMethod != HttpMethod.Put
                   && httpMethod.Method.ToUpperInvariant() != "PATCH";
        }

        public static bool HasBody(this HttpMethod httpMethod)
        {
            return httpMethod == HttpMethod.Post
                   || httpMethod == HttpMethod.Put
                   || httpMethod.Method.ToUpperInvariant() == "PATCH";
        }
    }
}
