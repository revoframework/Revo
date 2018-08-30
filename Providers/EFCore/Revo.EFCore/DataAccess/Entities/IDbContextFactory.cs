using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Entities
{
    public interface IDbContextFactory
    {
        DbContext CreateContext(string schemaSpace);
    }
}
