using System.Collections.Generic;
using Revo.Core.ValueObjects;

namespace Revo.Core.Types
{
    public class VersionedTypeId : ValueObject<VersionedTypeId>
    {
        public VersionedTypeId(string name, int version)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; }
        public int Version { get; }
        
        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            yield return (nameof(Name), Name);
            yield return (nameof(Version), Version);
        }
    }
}
