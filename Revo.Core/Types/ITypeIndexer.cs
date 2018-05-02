using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revo.Core.Types
{
    public interface ITypeIndexer
    {
        IEnumerable<VersionedType> IndexTypes<TBase>();
    }
}
