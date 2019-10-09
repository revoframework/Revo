using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Revo.EFCore.DataAccess.Entities;

namespace Revo.EFCore.DataAccess.Conventions
{
    public class BaseTypeAttributeConvention : EFCoreConventionBase
    {

        public override void Initialize(ModelBuilder modelBuilder)
        {
        }

        public override void Finalize(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var baseTypeAttribute = entity.ClrType.GetCustomAttribute<BaseTypeAttribute>(false);
                if (baseTypeAttribute != null)
                {
                    entity.BaseType = baseTypeAttribute.BaseType != null
                        ? modelBuilder.Model.FindEntityType(baseTypeAttribute.BaseType)
                        : null;
                }
            }
        }
    }
}
