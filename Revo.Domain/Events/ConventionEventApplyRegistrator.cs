using Revo.Domain.Entities;

namespace Revo.Domain.Events
{
    public class ConventionEventApplyRegistrator : IEventRouterRegistrator
    {
        public void RegisterEvents<T>(T self, IAggregateEventRouter router)
            where T : IComponent
        {
            var delegates = ConventionEventApplyRegistratorCache.GetApplyDelegates(self.GetType());
            foreach (var delegatePair in delegates)
            {
                router.Register(delegatePair.Key, ev => delegatePair.Value(self, ev));
            }
        }
    }
}
