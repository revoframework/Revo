using System;
using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Conventions
{
    public interface IEFCoreConvention : IComparable<IEFCoreConvention>
    {
        int Order { get; set; }
        void Initialize(ModelBuilder modelBuilder);
        void Finalize(ModelBuilder modelBuilder);
    }
}
