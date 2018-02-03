using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Revo.Infrastructure.Sagas
{
    public class SagaMetadata
    {
        public SagaMetadata(IEnumerable<KeyValuePair<string, string>> keys)
        {
            Keys = new ReadOnlyDictionary<string, string>(keys.ToDictionary(x => x.Key, x => x.Value));
        }
        
        public ReadOnlyDictionary<string, string> Keys { get; }
    }
}
