using System.Data.Entity;

namespace Revo.DataAccess.EF6.Entities
{
    public interface IDbContextFactory
    {
        DbContext CreateContext(string schemaSpace);
    }
}
