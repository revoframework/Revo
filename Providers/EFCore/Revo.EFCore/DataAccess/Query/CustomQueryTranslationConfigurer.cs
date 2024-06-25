using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Revo.Core.Core;
using Revo.EFCore.DataAccess.Configuration;

namespace Revo.EFCore.DataAccess.Query
{
    public class CustomQueryTranslationConfigurer(IServiceLocator serviceLocator) : IEFCoreConfigurer
    {
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