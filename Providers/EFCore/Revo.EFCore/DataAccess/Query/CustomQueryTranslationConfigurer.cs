using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Revo.Core.Core;
using Revo.EFCore.DataAccess.Configuration;

namespace Revo.EFCore.DataAccess.Query
{
    public class CustomQueryTranslationConfigurer : IEFCoreConfigurer
    {
        private readonly IServiceLocator serviceLocator;

        public CustomQueryTranslationConfigurer(IServiceLocator serviceLocator)
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
                .ReplaceService<IQueryTranslationPreprocessorFactory, CustomQueryTranslationPreprocessorFactory>();
        }
    }
}