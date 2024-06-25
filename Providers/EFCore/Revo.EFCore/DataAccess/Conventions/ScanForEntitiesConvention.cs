using System;
using Microsoft.EntityFrameworkCore;
using Revo.EFCore.DataAccess.Model;

namespace Revo.EFCore.DataAccess.Conventions
{
    public class ScanForEntitiesConvention : EFCoreConventionBase
    {
        private readonly EntityTypeDiscovery entityTypeDiscovery;

        public ScanForEntitiesConvention(EntityTypeDiscovery entityTypeDiscovery)
        {
            this.entityTypeDiscovery = entityTypeDiscovery;
        }
        
        public override void Initialize(ModelBuilder modelBuilder)
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

        public override void Finalize(ModelBuilder modelBuilder)
        {
        }
    }
}
