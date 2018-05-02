using System;
using System.Collections.Generic;
using System.Reflection;

namespace Revo.Core.Core
{
    public interface ITypeExplorer
    {
        IEnumerable<Type> GetAllTypes();
        IEnumerable<Assembly> GetAllReferencedAssemblies();
        Type FindType(string typeName);
    }
}
