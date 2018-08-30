namespace Revo.DataAccess.Entities
{
    public interface IHasClassId<TClassId>
    {
        TClassId ClassId { get; }
    }
}
