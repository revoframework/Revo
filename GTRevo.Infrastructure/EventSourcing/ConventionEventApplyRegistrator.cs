using GTRevo.Infrastructure.Domain;

namespace GTRevo.Infrastructure.EventSourcing
{
    public class ConventionEventApplyRegistrator : IEventRouterRegistrator
    {
        public void RegisterEvents<T>(T self, AggregateEventRouter router)
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
