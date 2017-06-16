namespace GTRevo.Infrastructure.Domain.EventSourcing
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