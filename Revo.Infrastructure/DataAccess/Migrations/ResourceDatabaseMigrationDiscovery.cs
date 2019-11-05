using System.Collections.Generic;
using System.Reflection;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class ResourceDatabaseMigrationDiscovery : IDatabaseMigrationDiscovery
    {
        private readonly ResourceDatabaseMigrationDiscoveryAssembly[] registeredAssemblies;

        public ResourceDatabaseMigrationDiscovery(ResourceDatabaseMigrationDiscoveryAssembly[] registeredAssemblies)
        {
            this.registeredAssemblies = registeredAssemblies;
        }

        public IEnumerable<IDatabaseMigration> DiscoverMigrations()
        {
            foreach (var registeredAssembly in registeredAssemblies)
            {
                var assembly = Assembly.Load(registeredAssembly.AssemblyName);
                var pathPrefix = assembly.GetName().Name.ToLowerInvariant() + "." + registeredAssembly.DirectoryPath
                    .Trim('/', '\\').Replace('/', '.').Replace('\\', '.')
                    .ToLowerInvariant()
                    + ".";

                foreach (var resourceName in assembly.GetManifestResourceNames())
                {
                    string resourceNameLower = resourceName.ToLowerInvariant();
                    if (resourceNameLower.StartsWith(pathPrefix))
                    {
                        string fileName = resourceName.Substring(pathPrefix.Length);

                        if (registeredAssembly.FileNameRegex.Match(fileName).Success)
                        {
                            yield return new ResourceFileSqlDatabaseMigration(assembly, resourceName, fileName, registeredAssembly.FileNameRegex);
                        }
                    }
                }
            }
        }
    }
}