using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Conventions
{
    public class IdColumnsPrefixedWithTableNameConvention : IEFCoreConvention
    {
        public void Initialize(ModelBuilder modelBuilder)
        {
        }

        public void Finalize(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    if (property.IsPrimaryKey() && property.Name == "Id")
                    {
                        property.Relational().ColumnName = $"{entity.ClrType.Name}Id";
                    }
                }
            }
        }
    }
}
