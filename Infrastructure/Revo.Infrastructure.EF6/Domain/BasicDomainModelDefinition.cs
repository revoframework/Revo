using System.Data.Entity;
using Revo.DataAccess.EF6.Model;
using Revo.Domain.Entities.Basic;

namespace Revo.Infrastructure.EF6.Domain
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
