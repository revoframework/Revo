using System.Collections.Generic;

namespace Revo.Core.Types
{
    public interface ITypeIndexer
    {
        IEnumerable<VersionedType> IndexTypes<TBase>();
    }
}
