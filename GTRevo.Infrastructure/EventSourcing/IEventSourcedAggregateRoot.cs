using GTRevo.Infrastructure.Domain;

namespace GTRevo.Infrastructure.EventSourcing
{
    public interface IEventSourcedAggregateRoot : IAggregateRoot
    {
        void LoadState(AggregateState state);
    }

    /*public interface ISnapshotAggregateRoot : IEventSourcedAggregateRoot
    {
        // TODO
    }*/
}