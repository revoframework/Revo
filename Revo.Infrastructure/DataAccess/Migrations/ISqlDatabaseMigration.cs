namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface ISqlDatabaseMigration : IDatabaseMigration
    {
        string[] SqlCommands { get; }
    }
}