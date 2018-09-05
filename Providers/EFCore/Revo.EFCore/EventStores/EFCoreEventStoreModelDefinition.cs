using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Revo.EFCore.DataAccess.Conventions;
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
