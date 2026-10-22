using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
#if NETCOREAPP3_1
using Microsoft.EntityFrameworkCore.Query.Internal;
#else
using Microsoft.EntityFrameworkCore.Query;
#endif
using Revo.EFCore.DataAccess.Configuration;

namespace Revo.EFCore.DataAccess.Query
{
    public class CustomQueryProviderConfigurer : IEFCoreConfigurer
    {
        private readonly IServiceProvider serviceProvider;

        public CustomQueryProviderConfigurer(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = (IDbContextOptionsBuilderInfrastructure)optionsBuilder;

            var extension = optionsBuilder.Options
                                .FindExtension<CustomQueryDbContextOptionsExtension>()
                            ?? new CustomQueryDbContextOptionsExtension(serviceProvider);
            builder.AddOrUpdateExtension(extension);

            optionsBuilder
                .ReplaceService<IAsyncQueryProvider, CustomQueryProvider>();
        }
    }
}
