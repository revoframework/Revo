namespace Revo.Core.Events
{
    public class PublishEventBufferFactory : IPublishEventBufferFactory
    {
        private readonly IEventBus eventBus;

        public PublishEventBufferFactory(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public IPublishEventBuffer CreatEventBuffer()
        {
            return new PublishEventBuffer(eventBus);
        }
    }
}
