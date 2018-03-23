using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Revo.Core.IO.OData
{
    public class ODataResult<T>
    {
        public ODataResult(List<T> value)
        {
            Value = value;
        }

        [JsonProperty("value")]
        public List<T> Value { get; private set; }
    }
}
