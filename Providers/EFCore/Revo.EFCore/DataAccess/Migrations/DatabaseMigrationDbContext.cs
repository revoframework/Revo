using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Migrations
{
    public class DatabaseMigrationDbContext : DbContext
    {
        public DatabaseMigrationDbContext(DbContextOptions<DatabaseMigrationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}