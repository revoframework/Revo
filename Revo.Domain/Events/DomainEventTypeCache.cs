using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Core;
using Revo.Core.Core.Lifecycle;
using Revo.Domain.Events.Attributes;

namespace Revo.Domain.Events
{
    public class DomainEventTypeCache : IApplicationStartListener, IDomainEventTypeCache
    {
        private readonly ITypeExplorer typeExplorer;
        private readonly Dictionary<(string eventName, int eventVersion), Type> eventNamesToTypes = new Dictionary<(string eventName, int eventVersion), Type>();
        private readonly Dictionary<Type, (string eventName, int eventVersion)> eventTypesToNames = new Dictionary<Type, (string eventName, int eventVersion)>();

        public DomainEventTypeCache(ITypeExplorer typeExplorer)
        {
            this.typeExplorer = typeExplorer;
        }
        
        public void OnApplicationStarted()
        {
            ExploreEventTypes();
        }

        public Type GetClrEventType(string eventName, int eventVersion)
        {
            Type eventType;
            if (!eventNamesToTypes.TryGetValue((eventName, eventVersion), out eventType))
            {
                throw new ArgumentException($"Could not find a domain event type named '{eventName}' (version {eventVersion})");
            }

            return eventType;
        }

        public (string eventName, int eventVersion) GetEventNameAndVersion(Type clrEventType)
        {
            if (!eventTypesToNames.TryGetValue(clrEventType, out var eventNameAndVersion))
            {
                throw new ArgumentException($"Could not find a domain event for type name '{clrEventType.FullName}'");
            }

            return eventNameAndVersion;
        }

        private void ExploreEventTypes()
        {
            var eventTypes = typeExplorer.GetAllTypes()
                .Where(x => typeof(DomainEvent).IsAssignableFrom(x))
                .Where(x => !x.IsAbstract && !x.IsGenericTypeDefinition);

            foreach (Type eventType in eventTypes)
            {
                EventVersionAttribute versionAttribute = (EventVersionAttribute)eventType
                    .GetCustomAttributes(typeof(EventVersionAttribute), false)
                    .FirstOrDefault();

                int version;
                string name;

                ExtractVersionFromName(eventType.Name, out name, out version);

                if (versionAttribute != null)
                {
                    version = versionAttribute.Version;
                }

                eventNamesToTypes[(name, version)] = eventType;
                eventTypesToNames[eventType] = (name, version);
            }
        }

        private void ExtractVersionFromName(string eventName, out string realEventName, out int version)
        {
            version = 1;
            realEventName = eventName;

            int versionBegin = eventName.Length;
            while (versionBegin > 1 && char.IsDigit(eventName[versionBegin - 1]))
            {
                versionBegin--;
            }

            if (versionBegin < 2 || eventName[versionBegin - 1] != 'V')
            {
                return;
            }

            realEventName = eventName.Substring(0, versionBegin - 1);
            version = int.Parse(eventName.Substring(versionBegin));
        }
    }
}
