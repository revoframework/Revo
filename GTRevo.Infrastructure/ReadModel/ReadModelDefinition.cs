using System.Data.Entity;
using GTRevo.DataAccess.EF6;

namespace GTRevo.Infrastructure.ReadModel
{
    public class ReadModelDefinition : IModelDefinition
    {
        public void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Types<IRowVersioned>()
                .Configure(x => x
                    .Property(y => y.Version)
                    .IsConcurrencyToken());
        }
    }
}
