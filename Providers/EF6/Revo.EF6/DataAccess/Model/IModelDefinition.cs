using System.Data.Entity;

namespace Revo.Infrastructure.EF6.DataAcccess.Model
{
    public interface IModelDefinition
    {
        void OnModelCreating(DbModelBuilder modelBuilder);
    }
}
