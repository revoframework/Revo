using System;

namespace Revo.Extensions.Notifications
{
    public interface INotificationTypeCache
    {
        Type GetClrNotificationType(string changeDataTypeName);
        string GetNotificationTypeName(Type clrType);
    }
}