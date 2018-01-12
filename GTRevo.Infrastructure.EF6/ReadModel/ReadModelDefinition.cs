using System.Data.Entity;
using GTRevo.DataAccess.EF6.Model;
using GTRevo.DataAccess.Entities;

namespace GTRevo.Infrastructure.EF6.ReadModel
{
    public class ReadModelDefinition : IModelDefinition
    {
        public void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Types<IRowVersioned>()
                .Configure(x => x
                    .Property(y => y.Version)
                    .IsConcurrencyToken());

            modelBuilder.Types<IManuallyRowVersioned>()
                .Configure(x => x
                    .Property(y => y.Version)
                    .IsConcurrencyToken());
        }
    }
}
