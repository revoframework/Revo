namespace Revo.EasyNetQ
{
    public interface IEasyNetQBlockingSubscriptionHandler
    {
        void HandleMessage(object message);
    }
}