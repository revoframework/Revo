using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Core.Types;

namespace Revo.Infrastructure.Events
{
    public interface IEventSerializer
    {
        IEvent DeserializeEvent(string eventJson, VersionedTypeId typeId);
        (string EventJson, VersionedTypeId TypeId) SerializeEvent(IEvent @event);
        string SerializeEventMetadata(IReadOnlyDictionary<string, string> metadata);
        IReadOnlyDictionary<string, string> DeserializeEventMetadata(string metadataJson);
    }
}