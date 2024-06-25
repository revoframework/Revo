using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Core;

namespace Revo.EFCore.DataAccess.Query
{
    public class CustomQueryDbContextOptionsExtension : IDbContextOptionsExtension
    {
        private readonly IServiceLocator serviceLocator;

        public CustomQueryDbContextOptionsExtension(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public void ApplyServices(IServiceCollection services)
        {
            services.AddSingleton(serviceLocator);
        }

        public void Validate(IDbContextOptions options)
        {
        }

        public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

        sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            public override bool IsDatabaseProvider => false;

            public override int GetServiceProviderHashCode() => 0;
            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;
            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
                => debugInfo[nameof(CustomQueryDbContextOptionsExtension)] = "1";

            public override string LogFragment => "using Revo CustomQueryDbContextOptionsExtension";
        }
    }
}