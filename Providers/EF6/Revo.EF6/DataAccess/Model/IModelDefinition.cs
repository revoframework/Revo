using System.Data.Entity;

namespace Revo.EF6.DataAccess.Model
{
    public interface IModelDefinition
    {
        void OnModelCreating(DbModelBuilder modelBuilder);
    }
}
