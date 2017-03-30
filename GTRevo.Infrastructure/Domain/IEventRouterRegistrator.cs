namespace GTRevo.Infrastructure.Domain
{
    public interface IEventRouterRegistrator
    {
        void RegisterEvents<T>(T self, IAggregateEventRouter router) where T : IComponent;
    }
}