using Microsoft.EntityFrameworkCore;
using Revo.EFCore.DataAccess.Model;
using Revo.Infrastructure.EventStores.Generic.Model;

namespace Revo.EFCore.EventStores
{
    public class EFCoreEventStoreModelDefinition : IEFCoreModelDefinition
    {
        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventStreamRow>()
                .Property(x => x.GlobalSequenceNumber)
                .ValueGeneratedOnAddOrUpdate();
        }
    }
}
