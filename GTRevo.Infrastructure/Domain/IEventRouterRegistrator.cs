namespace GTRevo.Infrastructure.Domain
{
    public interface IEventRouterRegistrator
    {
        void RegisterEvents<T>(T self, AggregateEventRouter router) where T : IComponent;
    }
}