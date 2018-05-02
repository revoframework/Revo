using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revo.Core.Types
{
    public interface IVersionedTypeRegistry
    {
        IEnumerable<VersionedType> GetAllTypes<TBase>();
        VersionedType GetTypeInfo<TBase>(VersionedTypeId id);
        VersionedType GetTypeInfo<TBase>(Type type);
        IReadOnlyCollection<VersionedType> GetTypeVersions<TBase>(string name);
        void ClearCache<TBase>();
    }
}
