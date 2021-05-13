namespace Revo.Domain.ReadModel
{
    /// <summary>
    /// Entity read model that separately keeps track of the last event projected and another arbitrary
    /// version number for arbitrary concurrency control. This may prove useful if you need to update
    /// the read model based on other sources than just the event stream of its source aggregate.
    /// </summary>
    public abstract class EventEntityReadModel : EntityReadModel, IEventNumberVersioned
    {
        public int EventNumber { get; set; }
    }
}
