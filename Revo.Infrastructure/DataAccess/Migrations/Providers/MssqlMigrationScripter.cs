namespace Revo.Infrastructure.DataAccess.Migrations.Providers
{
    public class MssqlMigrationScripter : GenericSqlDatabaseMigrationScripter
    {
        public MssqlMigrationScripter() : base("mssql")
        {
        }

        public override string CreateMigrationSchemaSql => $@"
CREATE TABLE [dbo].[{DatabaseMigrationRecordTable}] (
	[{RecordIdColumn}] [UNIQUEIDENTIFIER] NOT NULL,
	[{TimeAppliedColumn}] [DATETIME] NOT NULL,
	[{ModuleNameColumn}] [NVARCHAR] (MAX) NOT NULL,
	[{VersionColumn}] [NVARCHAR] (MAX),
	[{ChecksumColumn}] [NVARCHAR] (MAX) NOT NULL,
	[{FileNameColumn}] [NVARCHAR] (MAX)
	CONSTRAINT [{DatabaseMigrationRecordTable}_PK] PRIMARY KEY NONCLUSTERED ([{RecordIdColumn}])
);";
    }
}