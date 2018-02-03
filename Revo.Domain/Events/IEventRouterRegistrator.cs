using Revo.Domain.Entities;

namespace Revo.Domain.Events
{
    public interface IEventRouterRegistrator
    {
        void RegisterEvents<T>(T self, IAggregateEventRouter router) where T : IComponent;
    }
}