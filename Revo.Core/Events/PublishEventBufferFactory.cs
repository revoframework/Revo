namespace Revo.Core.Events
{
    public class PublishEventBufferFactory(IEventBus eventBus) : IPublishEventBufferFactory
    {
        private readonly IEventBus eventBus = eventBus;

        public IPublishEventBuffer CreateEventBuffer() => new PublishEventBuffer(eventBus);
    }
}
