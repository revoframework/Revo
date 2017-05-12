using System.Data.Entity;
using GTRevo.DataAccess.EF6;
using GTRevo.Infrastructure.EventSourcing.EF6.Model;

namespace GTRevo.Infrastructure.EventSourcing.EF6
{
    public class EF6EventSourcingModelDefinition : IModelDefinition
    {
        public void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
