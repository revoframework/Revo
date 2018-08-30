using System;
using Microsoft.EntityFrameworkCore;
using Revo.EFCore.DataAccess.Model;

namespace Revo.EFCore.DataAccess.Conventions
{
    public class ScanForEntitiesConvention : IEFCoreConvention
    {
        private readonly EntityTypeDiscovery entityTypeDiscovery;

        public ScanForEntitiesConvention(EntityTypeDiscovery entityTypeDiscovery)
        {
            this.entityTypeDiscovery = entityTypeDiscovery;
        }

        public void Initialize(ModelBuilder modelBuilder)
        {
            var all = entityTypeDiscovery.DiscoverEntities();
            foreach (var schemaSpaceAndEntities in all)
            {
                foreach (Type entityType in schemaSpaceAndEntities.Value)
                {
                    modelBuilder.Entity(entityType);
                }
            }
        }

        public void Finalize(ModelBuilder modelBuilder)
        {
        }
    }
}
