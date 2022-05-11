using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
#if NETCOREAPP3_1
using Microsoft.EntityFrameworkCore.Query.Internal;
#else
using Microsoft.EntityFrameworkCore.Query;
#endif
using Revo.Core.Core;
using Revo.EFCore.DataAccess.Configuration;

namespace Revo.EFCore.DataAccess.Query
{
    public class CustomQueryProviderConfigurer : IEFCoreConfigurer
    {
        private readonly IServiceLocator serviceLocator;

        public CustomQueryProviderConfigurer(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }
        
        public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = (IDbContextOptionsBuilderInfrastructure)optionsBuilder;

            var extension = optionsBuilder.Options
                                .FindExtension<CustomQueryDbContextOptionsExtension>()
                            ?? new CustomQueryDbContextOptionsExtension(serviceLocator);
            builder.AddOrUpdateExtension(extension);

            optionsBuilder
                .ReplaceService<IAsyncQueryProvider, CustomQueryProvider>();
        }
    }
}