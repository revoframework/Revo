namespace GTRevo.Infrastructure.Domain
{
    public interface IHasId<TId>
    {
        TId Id { get; }
    }
}
