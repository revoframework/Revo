namespace GTRevo.Infrastructure.Core.Domain
{
    public interface IHasId<TId>
    {
        TId Id { get; }
    }
}
