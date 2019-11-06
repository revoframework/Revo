using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class FileDatabaseMigrationDiscovery : IDatabaseMigrationDiscovery
    {
        private readonly FileDatabaseMigrationDiscoveryPath[] paths;

        public FileDatabaseMigrationDiscovery(FileDatabaseMigrationDiscoveryPath[] paths)
        {
            this.paths = paths.Distinct().ToArray();
        }

        public IEnumerable<IDatabaseMigration> DiscoverMigrations()
        {
            foreach (var path in paths)
            {
                if (!Directory.Exists(path.ScanRootPath))
                {
                    continue;
                }

                var files = Directory.EnumerateFiles(path.ScanRootPath, "*.*",
                    path.ScanRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    if (path.FileNameRegex.Match(fileName).Success)
                    {
                        yield return new DiskFileSqlDatabaseMigration(file, path.FileNameRegex);
                    }
                }
            }
        }
    }
}