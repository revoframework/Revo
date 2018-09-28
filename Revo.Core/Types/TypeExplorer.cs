using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Revo.Core.Types
{
    public class TypeExplorer : ITypeExplorer
    {
        public virtual IEnumerable<Assembly> GetAllReferencedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        public IEnumerable<Type> GetAllTypes()
        {
            var assemblies = GetAllReferencedAssemblies();
            return assemblies.SelectMany(x => x.GetTypes());
        }

        public Type FindType(string typeName)
        {
            foreach (var type in GetAllTypes())
            {
                if (type.FullName == typeName)
                    return type;
            }
            return null;
        }
    }
}
