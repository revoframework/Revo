using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Conventions
{
    public abstract class EFCoreConventionBase : IEFCoreConvention
    {
        public int Order { get; set; }
        public int CompareTo(IEFCoreConvention other)
        {
            return Order.CompareTo(other.Order);
        }

        public abstract void Initialize(ModelBuilder modelBuilder);
        public abstract void Finalize(ModelBuilder modelBuilder);
    }
}
