namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface IDatabaseMigrationSelectorOptions
    {
        bool RerunRepeatableMigrationsOnDependencyUpdate { get; set; }
    }
}