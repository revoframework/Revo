namespace Revo.DataAccess.Entities
{
    public interface IManuallyRowVersioned
    {
        int Version { get; set; }
    }
}
