using System;

namespace GTRevo.Infrastructure.Notifications
{
    public interface INotificationTypeCache
    {
        Type GetClrNotificationType(string changeDataTypeName);
        string GetNotificationTypeName(Type clrType);
    }
}