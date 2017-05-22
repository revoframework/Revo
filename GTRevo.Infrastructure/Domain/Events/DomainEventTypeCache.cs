using System;
using System.Collections.Generic;
using System.Linq;
using GTRevo.Infrastructure.Domain.Attributes;
using GTRevo.Platform.Core;
using GTRevo.Platform.Core.Lifecycle;

namespace GTRevo.Infrastructure.Domain.Events
{
    public class DomainEventTypeCache : IApplicationStartListener
    {
        private readonly ITypeExplorer typeExplorer;
        private readonly Dictionary<Tuple<string, int>, Type> eventNamesToTypes = new Dictionary<Tuple<string, int>, Type>();
        private readonly Dictionary<Type, Tuple<string, int>> eventTypesToNames = new Dictionary<Type, Tuple<string, int>>();

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
            if (!eventNamesToTypes.TryGetValue(new Tuple<string, int>(eventName, eventVersion), out eventType))
            {
                throw new ArgumentException($"Could not find a domain event type named '{eventName}' (version {eventVersion})");
            }

            return eventType;
        }

        public Tuple<string, int> GetEventNameAndVersion(Type clrEventType)
        {
            Tuple<string, int> eventNameAndVersion;
            if (!eventTypesToNames.TryGetValue(clrEventType, out eventNameAndVersion))
            {
                throw new ArgumentException($"Could not find a domain event for type name '{clrEventType.FullName}'");
            }

            return eventNameAndVersion;
        }

        private void ExploreEventTypes()
        {
            var eventTypes = typeExplorer.GetAllTypes()
                .Where(x => typeof(DomainEvent).IsAssignableFrom(x))
                .Where(x => !x.IsAbstract);

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

                eventNamesToTypes[new Tuple<string, int>(name, version)] = eventType;
                eventTypesToNames[eventType] = new Tuple<string, int>(name, version);
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
