namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationSelectorOptions : IDatabaseMigrationSelectorOptions
    {
        public bool RerunRepeatableMigrationsOnDependencyUpdate { get; set; } = true;
    }
}