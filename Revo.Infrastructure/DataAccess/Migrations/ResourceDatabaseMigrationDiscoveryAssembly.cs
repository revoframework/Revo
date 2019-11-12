using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Revo.Core.ValueObjects;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class ResourceDatabaseMigrationDiscoveryAssembly : ValueObject<ResourceDatabaseMigrationDiscoveryAssembly>
    {
        public ResourceDatabaseMigrationDiscoveryAssembly(Assembly assembly, string directoryPath,
            Regex fileNameRegex = null)
        {
            Assembly = assembly;
            DirectoryPath = directoryPath;
            FileNameRegex = fileNameRegex ?? FileSqlDatabaseMigration.DefaultFileNameRegex;
        }

        public Assembly Assembly { get; }
        public string DirectoryPath { get; }

        /// <summary>
        /// Regex for matching the file names. The default is
        /// ^(?&lt;module&gt;[a-z0-9\-]+)(?:_(?&lt;baseline&gt;baseline))?(?:_(?&lt;repeatable&gt;repeatable))?(?:_(?&lt;version&gt;\d+(?:\.\d+)*))?(?:_(?&lt;tag&gt;[a-z0-9\-]+)(?:,(?&lt;tag&gt;[a-z0-9\-]+))*)?\.sql$
        /// This matches files with .sql extension and also extracts some migration parameters from the file name.
        /// </summary>
        public Regex FileNameRegex { get; }

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            yield return (nameof(Assembly), Assembly);
            yield return (nameof(DirectoryPath), DirectoryPath);
            yield return (nameof(FileNameRegex), FileNameRegex.ToString());
        }
    }
}