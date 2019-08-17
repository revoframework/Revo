using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Conventions
{
    public class IdColumnsPrefixedWithTableNameConvention : EFCoreConventionBase
    {

        public override void Initialize(ModelBuilder modelBuilder)
        {
        }

        public override void Finalize(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (entity.BaseType != null)
                {
                    continue;
                }

                foreach (var property in entity.GetProperties())
                {
                    if (property.IsPrimaryKey() && property.Name == "Id")
                    {
                        property.SetColumnName($"{entity.ClrType.Name}Id");
                    }
                }
            }
        }
    }
}
