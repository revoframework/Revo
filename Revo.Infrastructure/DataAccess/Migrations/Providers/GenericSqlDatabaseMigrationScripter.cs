namespace Revo.Infrastructure.DataAccess.Migrations.Providers
{
    public class GenericSqlDatabaseMigrationScripter : IDatabaseMigrationScripter
    {
        public GenericSqlDatabaseMigrationScripter(string databaseTypeTag)
        {
            DatabaseTypeTag = databaseTypeTag;
        }
        
        public string DatabaseMigrationRecordTable { get; set; } = "REV_DATABASE_MIGRATION_RECORD";
        public string RecordIdColumn { get; set; } = "REV_DMR_DatabaseMigrationRecordId";
        public string TimeAppliedColumn { get; set; } = "REV_DMR_TimeApplied";
        public string ModuleNameColumn { get; set; } = "REV_DMR_ModuleName";
        public string VersionColumn { get; set; } = "REV_DMR_Version";
        public string ChecksumColumn { get; set; } = "REV_DMR_Checksum";
        public string FileNameColumn { get; set; } = "REV_DMR_FileName";

        public string DatabaseTypeTag { get; }

        public virtual string SelectMigrationRecordsSql => $@"
SELECT
    {RecordIdColumn} AS id,
    {TimeAppliedColumn} AS time_applied,
    {ModuleNameColumn} AS module_name,
    {VersionColumn} AS version,
    {ChecksumColumn} AS checksum,
    {FileNameColumn} AS file_name
FROM {DatabaseMigrationRecordTable}
ORDER BY {TimeAppliedColumn};
";

        public virtual string SelectMigrationSchemaExistsSql => $@"
SELECT COUNT(*)
FROM information_schema.tables
WHERE table_name = '{DatabaseMigrationRecordTable}';";

        public virtual string CreateMigrationSchemaSql => $@"
CREATE TABLE {DatabaseMigrationRecordTable} (
    {RecordIdColumn} CHAR(36) NOT NULL,
    {TimeAppliedColumn} TIMESTAMP NOT NULL,
    {ModuleNameColumn} VARCHAR (MAX) NOT NULL,
    {VersionColumn} VARCHAR (MAX),
    {ChecksumColumn} VARCHAR (MAX) NOT NULL,
    {FileNameColumn} VARCHAR (MAX)
    CONSTRAINT {DatabaseMigrationRecordTable}_PK PRIMARY KEY({RecordIdColumn})
);";

        public string InsertMigrationRecordSql => $@"
INSERT INTO {DatabaseMigrationRecordTable} ({RecordIdColumn}, {TimeAppliedColumn}, {ModuleNameColumn}, {VersionColumn}, {ChecksumColumn}, {FileNameColumn})
VALUES (@Id, @TimeApplied, @ModuleName, @Version, @Checksum, @FileName)";

    }
}