namespace Revo.Core.Events
{
    public class PublishEventBufferFactory : IPublishEventBufferFactory
    {
        private readonly IEventBus eventBus;

        public PublishEventBufferFactory(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public IPublishEventBuffer CreateEventBuffer()
        {
            return new PublishEventBuffer(eventBus);
        }
    }
}
