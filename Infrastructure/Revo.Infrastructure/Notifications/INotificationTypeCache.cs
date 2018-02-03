using System;

namespace Revo.Infrastructure.Notifications
{
    public interface INotificationTypeCache
    {
        Type GetClrNotificationType(string changeDataTypeName);
        string GetNotificationTypeName(Type clrType);
    }
}