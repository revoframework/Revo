namespace Revo.Infrastructure.DataAccess.Migrations.Providers
{
    public interface IDatabaseMigrationScripter
    {
        string DatabaseMigrationRecordTable { get; }
        string RecordIdColumn { get; }
        string TimeAppliedColumn { get; }
        string ModuleNameColumn { get; }
        string VersionColumn { get; }
        string ChecksumColumn { get; }
        string FileNameColumn { get; }
        string DatabaseTypeTag { get; }

        string SelectMigrationRecordsSql { get; }
        string SelectMigrationSchemaExistsSql { get; }
        string CreateMigrationSchemaSql { get; }
        string InsertMigrationRecordSql { get; }
    }
}