namespace Revo.Core.Events
{
    public interface IPublishEventBufferFactory
    {
        IPublishEventBuffer CreateEventBuffer();
    }
}