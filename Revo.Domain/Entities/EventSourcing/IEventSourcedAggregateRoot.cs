namespace Revo.Domain.Entities.EventSourcing
{
    public interface IEventSourcedAggregateRoot : IAggregateRoot
    {
        void LoadState(AggregateState state);
    }
}