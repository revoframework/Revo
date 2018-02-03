using System.Data.Entity;
using Revo.DataAccess.EF6.Model;

namespace Revo.Infrastructure.EF6.EventStore
{
    public class EF6EventSourcingModelDefinition : IModelDefinition
    {
        public void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
