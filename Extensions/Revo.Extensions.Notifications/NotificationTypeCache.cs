using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Lifecycle;
using Revo.Core.Types;

namespace Revo.Extensions.Notifications
{
    public class NotificationTypeCache(ITypeExplorer typeExplorer) : IApplicationConfigurer, INotificationTypeCache
    {
        private Dictionary<string, Type> namesToTypes;

        public void Configure()
        {
            EnsureLoaded();
        }

        public Type GetClrNotificationType(string changeDataTypeName)
        {
            EnsureLoaded();

            Type changeDataType;
            if (!namesToTypes.TryGetValue(changeDataTypeName, out changeDataType))
            {
                throw new ArgumentException($"Could not find a notification type named '{changeDataTypeName}'");
            }

            return changeDataType;
        }

        public string GetNotificationTypeName(Type clrType)
        {
            if (!typeof(INotification).IsAssignableFrom(clrType)
                || clrType.IsAbstract)
            {
                throw new ArgumentException($"Invalid notification type: {clrType.FullName}");
            }

            return clrType.FullName;
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
                .Where(x => x.IsClass)
                .Where(x => typeof(INotification).IsAssignableFrom(x))
                .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition);

            foreach (Type changeDataType in changeDataTypes)
            {
                namesToTypes[changeDataType.FullName] = changeDataType;
            }
        }
    }
}
