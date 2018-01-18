namespace GTRevo.Infrastructure.Core.Domain.EventSourcing
{
    public interface IEventSourcedAggregateRoot : IAggregateRoot
    {
        void LoadState(AggregateState state);
    }
}