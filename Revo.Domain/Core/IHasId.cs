namespace Revo.Domain.Core
{
    public interface IHasId<TId>
    {
        TId Id { get; }
    }
}
