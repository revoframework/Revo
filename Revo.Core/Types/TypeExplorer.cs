using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;

namespace Revo.Core.Types
{
    public class TypeExplorer : ITypeExplorer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public virtual IEnumerable<Assembly> GetAllReferencedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        public IEnumerable<Type> GetAllTypes()
        {
            var assemblies = GetAllReferencedAssemblies();
            return assemblies.SelectMany(x =>
            {
                try
                {
                    return x.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    Logger.Warn(e, $"Could not load types from assembly {x}, loader exceptions: {string.Join("\n", e.LoaderExceptions.Select(x => x.ToString()))}");
                    return new Type[0];
                }
            });
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
