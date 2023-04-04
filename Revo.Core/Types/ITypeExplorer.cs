using System;
using System.Collections.Generic;
using System.Reflection;

namespace Revo.Core.Types;

public interface ITypeExplorer
{
    IEnumerable<Type> GetAllTypes();
    IEnumerable<Assembly> GetAllAssemblies();
    Type FindType(string typeName);
}