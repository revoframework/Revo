using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Revo.EFCore.DataAccess.Configuration;

namespace Revo.EFCore.DataAccess.Query
{
    public class CustomQueryTranslationConfigurer : IEFCoreConfigurer
    {
        public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           // optionsBuilder
           //     .ReplaceService<IQueryTranslationPreprocessorFactory, CustomQueryTranslationPreprocessorFactory>();
        }
    }
}