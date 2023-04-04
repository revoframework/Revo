using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Revo.Core.Configuration;
using Revo.Core.Types;

namespace Revo.Infrastructure.DataAccess;

public class DatabaseInitializerDiscovery : IDatabaseInitializerDiscovery
{
    private readonly IServiceProvider serviceProvider;

    public DatabaseInitializerDiscovery(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public static void RegisterDiscoveredInitializers(IRevoConfiguration configuration,
        IServiceCollection serviceCollection)
    {
        var typeExplorer = new TypeExplorer(
            configuration.GetSection<CoreConfigurationSection>(),
            NullLogger<TypeExplorer>.Instance);

        var initializerTypes = typeExplorer.GetAllTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition
                        && typeof(IDatabaseInitializer).IsAssignableFrom(t));

        foreach (var implType in initializerTypes)
        {
            var alreadyRegistered = serviceCollection.Any(d =>
                d.ServiceType == typeof(IDatabaseInitializer) && d.ImplementationType == implType);

            if (!alreadyRegistered)
            {
                serviceCollection.AddTransient(typeof(IDatabaseInitializer), implType);
            }
        }
    }

    public IEnumerable<IDatabaseInitializer> DiscoverDatabaseInitializers()
    {
        return serviceProvider.GetServices<IDatabaseInitializer>();
    }
}
