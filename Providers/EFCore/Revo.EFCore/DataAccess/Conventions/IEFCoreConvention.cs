using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Conventions
{
    public interface IEFCoreConvention
    {
        void Initialize(ModelBuilder modelBuilder);
        void Finalize(ModelBuilder modelBuilder);
    }
}
