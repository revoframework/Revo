using System.Data.Entity;
using GTRevo.DataAccess.EF6.Model;

namespace GTRevo.Infrastructure.EF6.EventSourcing
{
    public class EF6EventSourcingModelDefinition : IModelDefinition
    {
        public void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
