using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Revo.Core.IO.OData
{
    public class ODataResultWithCount<T> : ODataResult<T>
    {
        public ODataResultWithCount(List<T> value, long count) : base(value)
        {
            Count = count;
        }

        [JsonProperty("count")]
        public long Count { get; private set; }
    }
}
