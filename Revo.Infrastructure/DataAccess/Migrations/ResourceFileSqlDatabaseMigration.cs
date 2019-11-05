using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class ResourceFileSqlDatabaseMigration : FileSqlDatabaseMigration
    {
        public ResourceFileSqlDatabaseMigration(Assembly assembly, string resourceName,
            string fileName, Regex fileNameRegex = null) : base(fileName, fileNameRegex)
        {
            Assembly = assembly;
            ResourceName = resourceName;
        }

        public Assembly Assembly { get; }
        public string ResourceName { get; }

        protected override string ReadSqlFileContents()
        {
            using (Stream resourceStream = Assembly.GetManifestResourceStream(ResourceName))
            {
                if (resourceStream == null)
                {
                    throw new DatabaseMigrationException($"Cannot read database migration file from assembly '{Assembly.FullName}' resource '{ResourceName}'");
                }

                using (StreamReader reader = new StreamReader(resourceStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}