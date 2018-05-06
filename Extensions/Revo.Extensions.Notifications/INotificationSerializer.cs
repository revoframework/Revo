namespace Revo.Extensions.Notifications
{
    public interface INotificationSerializer
    {
        INotification FromJson(SerializedNotification serializedNotification);
        SerializedNotification ToJson(INotification notification);
    }
}