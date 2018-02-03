namespace Revo.Infrastructure.Notifications
{
    public interface INotificationSerializer
    {
        INotification FromJson(SerializedNotification serializedNotification);
        SerializedNotification ToJson(INotification notification);
    }
}