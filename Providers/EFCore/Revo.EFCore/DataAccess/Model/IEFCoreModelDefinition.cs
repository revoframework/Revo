using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Model
{
    public interface IEFCoreModelDefinition
    {
        void OnModelCreating(ModelBuilder modelBuilder);
    }
}
