namespace Revo.Domain.ReadModel
{
    /// <summary>
    /// Read model keeping track of the last entity event projected.
    /// Useful if you need to update the read model based on other sources beside the event stream of a single aggregate.
    /// When combined with IManuallyRowVersioned, the Version field may then be used for any arbitrary concurrency control.
    /// </summary>
    public interface IEventNumberVersioned
    {
        int EventNumber { get; set; }
    }
}
