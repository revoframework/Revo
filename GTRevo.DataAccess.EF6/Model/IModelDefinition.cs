using System.Data.Entity;

namespace GTRevo.DataAccess.EF6.Model
{
    public interface IModelDefinition
    {
        void OnModelCreating(DbModelBuilder modelBuilder);
    }
}
