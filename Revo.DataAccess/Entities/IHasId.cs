namespace Revo.DataAccess.Entities
{
    public interface IHasId<TId>
    {
        TId Id { get; }
    }
}
