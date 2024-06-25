using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using Revo.Core.Core;

namespace Revo.EFCore.DataAccess.Query
{
    public class CustomQueryTranslationPreprocessorFactory(QueryTranslationPreprocessorDependencies dependencies,
            RelationalQueryTranslationPreprocessorDependencies relationalDependencies,
            IServiceLocator serviceLocator) : IQueryTranslationPreprocessorFactory
    {
        protected QueryTranslationPreprocessorDependencies Dependencies { get; } = dependencies;

        protected RelationalQueryTranslationPreprocessorDependencies RelationalDependencies = relationalDependencies;

        public QueryTranslationPreprocessor Create(QueryCompilationContext queryCompilationContext)
        {
            var translationPlugins = serviceLocator.GetAll<IQueryTranslationPlugin>();
            return new CustomQueryTranslationPreprocessor(Dependencies, RelationalDependencies,
                queryCompilationContext, translationPlugins.ToArray());
        }
    }
}