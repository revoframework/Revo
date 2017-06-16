namespace GTRevo.Infrastructure.Domain
{
    public interface IHasClassId<TClassId>
    {
        TClassId ClassId { get; }
    }
}
