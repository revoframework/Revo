using System.Collections.Generic;
using System.Linq;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class ResourceDatabaseMigrationDiscovery : IDatabaseMigrationDiscovery
    {
        private readonly ResourceDatabaseMigrationDiscoveryAssembly[] registeredAssemblies;

        public ResourceDatabaseMigrationDiscovery(ResourceDatabaseMigrationDiscoveryAssembly[] registeredAssemblies)
        {
            this.registeredAssemblies = registeredAssemblies.Distinct().ToArray();
        }

        public IEnumerable<IDatabaseMigration> DiscoverMigrations()
        {
            foreach (var registeredAssembly in registeredAssemblies)
            {
                var pathPrefix = registeredAssembly.Assembly.GetName().Name.ToLowerInvariant() + "." + registeredAssembly.DirectoryPath
                    .Trim('/', '\\').Replace('/', '.').Replace('\\', '.')
                    .ToLowerInvariant()
                    + ".";

                foreach (var resourceName in registeredAssembly.Assembly.GetManifestResourceNames())
                {
                    string resourceNameLower = resourceName.ToLowerInvariant();
                    if (resourceNameLower.StartsWith(pathPrefix))
                    {
                        string fileName = resourceName.Substring(pathPrefix.Length);

                        if (registeredAssembly.FileNameRegex.Match(fileName).Success)
                        {
                            yield return new ResourceFileSqlDatabaseMigration(registeredAssembly.Assembly, resourceName,
                                fileName, registeredAssembly.FileNameRegex);
                        }
                    }
                }
            }
        }
    }
}