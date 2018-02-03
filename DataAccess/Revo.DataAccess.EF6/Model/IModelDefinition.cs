using System.Data.Entity;

namespace Revo.DataAccess.EF6.Model
{
    public interface IModelDefinition
    {
        void OnModelCreating(DbModelBuilder modelBuilder);
    }
}
