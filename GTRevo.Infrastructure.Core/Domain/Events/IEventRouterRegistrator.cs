namespace GTRevo.Infrastructure.Core.Domain.Events
{
    public interface IEventRouterRegistrator
    {
        void RegisterEvents<T>(T self, IAggregateEventRouter router) where T : IComponent;
    }
}