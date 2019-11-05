using System.IO;
using System.Text.RegularExpressions;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DiskFileSqlDatabaseMigration : FileSqlDatabaseMigration
    {
        public DiskFileSqlDatabaseMigration(string filePath, Regex fileNameRegex = null) : base(Path.GetFileName(filePath), fileNameRegex)
        {
            FilePath = filePath;
        }

        public string FilePath { get; set; }

        protected override string ReadSqlFileContents()
        {
            return File.ReadAllText(FilePath);
        }
    }
}