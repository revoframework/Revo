using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Lifecycle;
using Revo.Core.Types;

namespace Revo.Extensions.History.ChangeTracking
{
    public class ChangeDataTypeCache : IApplicationStartedListener, IChangeDataTypeCache
    {
        private readonly ITypeExplorer typeExplorer;
        private Dictionary<string, Type> namesToTypes;

        public ChangeDataTypeCache(ITypeExplorer typeExplorer)
        {
            this.typeExplorer = typeExplorer;
        }

        public void OnApplicationStarted()
        {
            EnsureLoaded();
        }

        public Type GetClrChangeDataType(string changeDataTypeName)
        {
            EnsureLoaded();

            Type changeDataType = Type.GetType(changeDataTypeName);
            if (changeDataType != null && changeDataType.IsConstructedGenericType)
            {
                if (!namesToTypes.Values.Contains(changeDataType.GetGenericTypeDefinition()))
                {
                    throw new ArgumentException($"Could not find a channge data type named '{changeDataTypeName}'");
                }
            }
            else if (changeDataType == null || namesToTypes.Values.Contains(changeDataType))
            {
                throw new ArgumentException($"Could not find a channge data type named '{changeDataTypeName}'");
            }
            
            return changeDataType;
        }

        public string GetChangeDataTypeName(Type clrType)
        {
            if (!typeof(ChangeData).IsAssignableFrom(clrType)
                || clrType.IsAbstract)
            {
                throw new ArgumentException($"Invalid change data type: {clrType.FullName}");
            }

            return clrType.ToString(); //returns a shorter version than FullName
        }

        private void EnsureLoaded()
        {
            if (namesToTypes == null)
            {
                ExploreTypes();
            }
        }

        private void ExploreTypes()
        {
            namesToTypes = new Dictionary<string, Type>();

            var changeDataTypes = typeExplorer.GetAllTypes()
                .Where(x => typeof(ChangeData).IsAssignableFrom(x))
                .Where(x => !x.IsAbstract /*&& !x.IsGenericTypeDefinition*/);

            foreach (Type changeDataType in changeDataTypes)
            {
                namesToTypes[changeDataType.ToString()] = changeDataType;
            }
        }
    }
}
