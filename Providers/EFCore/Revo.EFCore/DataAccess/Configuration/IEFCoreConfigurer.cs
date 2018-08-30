using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Configuration
{
    public interface IEFCoreConfigurer
    {
        void OnConfiguring(DbContextOptionsBuilder optionsBuilder);
    }
}
