namespace Revo.Domain.Core
{
    public interface IHasClassId<TClassId>
    {
        TClassId ClassId { get; }
    }
}
