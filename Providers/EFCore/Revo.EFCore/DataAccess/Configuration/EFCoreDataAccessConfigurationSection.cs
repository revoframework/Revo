using System;
using Microsoft.EntityFrameworkCore;
using Revo.Core.Configuration;
using Revo.EFCore.DataAccess.Conventions;

namespace Revo.EFCore.DataAccess.Configuration
{
    public class EFCoreDataAccessConfigurationSection : IRevoConfigurationSection
    {
        public bool IsActive { get; set; }
        public bool UseAsPrimaryRepository { get; set; }
        public Type[] ConventionTypes { get; set; } = { typeof(ScanForEntitiesConvention) };
        public Action<DbContextOptionsBuilder> Configurer { get; set; }
    }
}
