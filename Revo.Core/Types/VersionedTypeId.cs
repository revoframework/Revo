using System.Collections.Generic;
using Revo.Core.ValueObjects;

namespace Revo.Core.Types
{
    public class VersionedTypeId(string name, int version) : ValueObject<VersionedTypeId>
    {

        public string Name { get; } = name;
        public int Version { get; } = version;

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            yield return (nameof(Name), Name);
            yield return (nameof(Version), Version);
        }
    }
}
