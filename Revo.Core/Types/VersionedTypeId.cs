using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revo.Core.Types
{
    public class VersionedTypeId
    {
        public VersionedTypeId(string name, int version)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; }
        public int Version { get; }

        public override string ToString()
        {
            return $"VersionedTypeId {{Name:{Name}, Version:{Version}}}";
        }

        public override bool Equals(object obj)
        {
            return obj is VersionedTypeId other
                   && other.Name == Name
                   && other.Version == Version;
        }

        public override int GetHashCode()
        {
            return (Name.GetHashCode() * 397) ^ Version.GetHashCode();
        }
    }
}
