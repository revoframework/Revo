using System.Data.Entity;
using GTRevo.DataAccess.EF6;

namespace GTRevo.Infrastructure.Domain.Basic
{
    public class BasicDomainModelDefinition : IModelDefinition
    {
        public void OnModelCreating(DbModelBuilder modelBuilder)
        {
         /*   modelBuilder.Types<BasicEntity>()
                .Configure(x => x
                    .Property(y => y.Version)
                    .IsConcurrencyToken());*/

            modelBuilder.Types<BasicAggregateRoot>()
                .Configure(x => x
                    .Property(y => y.Version)
                    .IsConcurrencyToken());
        }
    }
}
