using System.Data.Entity;

namespace GTRevo.DataAccess.EF6.Entities
{
    public interface IDbContextFactory
    {
        DbContext CreateContext(string schemaSpace);
    }
}
