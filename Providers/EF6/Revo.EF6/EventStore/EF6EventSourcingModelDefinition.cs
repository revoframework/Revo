using System.Data.Entity;
using Revo.Infrastructure.EF6.DataAcccess.Model;

namespace Revo.EF6.EventStore
{
    public class EF6EventSourcingModelDefinition : IModelDefinition
    {
        public void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
