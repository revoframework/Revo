using System.Data.Entity;

namespace GTRevo.DataAccess.EF6
{
    public interface IModelDefinition
    {
        void OnModelCreating(DbModelBuilder modelBuilder);
    }
}
