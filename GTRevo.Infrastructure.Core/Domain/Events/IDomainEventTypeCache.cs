using System;

namespace GTRevo.Infrastructure.Core.Domain.Events
{
    public interface IDomainEventTypeCache
    {
        Type GetClrEventType(string eventName, int eventVersion);
        Tuple<string, int> GetEventNameAndVersion(Type clrEventType);
    }
}