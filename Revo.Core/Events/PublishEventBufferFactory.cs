namespace Revo.Core.Events
{
    public class PublishEventBufferFactory(IEventBus eventBus) : IPublishEventBufferFactory
    {
        public IPublishEventBuffer CreateEventBuffer()
        {
            return new PublishEventBuffer(eventBus);
        }
    }
}
