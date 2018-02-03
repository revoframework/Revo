using System;

namespace Revo.Domain.Events
{
    public interface IDomainEventTypeCache
    {
        Type GetClrEventType(string eventName, int eventVersion);
        (string eventName, int eventVersion) GetEventNameAndVersion(Type clrEventType);
    }
}