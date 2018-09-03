using System.Data.Entity;
using Revo.DataAccess.Entities;
using Revo.EF6.DataAccess.Model;

namespace Revo.EF6.ReadModel
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
