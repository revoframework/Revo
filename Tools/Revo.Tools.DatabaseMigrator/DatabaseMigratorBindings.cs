using System.Collections.Generic;
using Revo.Infrastructure.DataAccess.Migrations;

namespace Revo.Tools.DatabaseMigrator;

/// <summary>
/// Collects migration discovery registrations from <see cref="IDatabaseMigratorPlugin"/> implementations.
/// </summary>
public sealed class DatabaseMigratorBindings
{
    private readonly List<ResourceDatabaseMigrationDiscoveryAssembly> resourceAssemblies = [];

    public void AddResourceDiscovery(ResourceDatabaseMigrationDiscoveryAssembly discovery)
    {
        resourceAssemblies.Add(discovery);
    }

    internal ResourceDatabaseMigrationDiscoveryAssembly[] GetResourceAssemblies() =>
        resourceAssemblies.ToArray();
}
