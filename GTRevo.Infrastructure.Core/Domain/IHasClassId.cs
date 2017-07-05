namespace GTRevo.Infrastructure.Core.Domain
{
    public interface IHasClassId<TClassId>
    {
        TClassId ClassId { get; }
    }
}
