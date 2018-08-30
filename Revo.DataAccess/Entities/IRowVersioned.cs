namespace Revo.DataAccess.Entities
{
    public interface IRowVersioned
    {
        int Version { get; set; }
    }
}
