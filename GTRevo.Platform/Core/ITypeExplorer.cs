using System;
using System.Collections.Generic;
using System.Reflection;

namespace GTRevo.Platform.Core
{
    public interface ITypeExplorer
    {
        IEnumerable<Type> GetAllTypes();
        IEnumerable<Assembly> GetAllReferencedAssemblies();
    }
}
