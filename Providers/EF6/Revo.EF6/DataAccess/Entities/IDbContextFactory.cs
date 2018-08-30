using System.Data.Entity;

namespace Revo.EF6.DataAccess.Entities
{
    public interface IDbContextFactory
    {
        DbContext CreateContext(string schemaSpace);
    }
}
