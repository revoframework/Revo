using Microsoft.EntityFrameworkCore.Query;

namespace Revo.EFCore.DataAccess.Query
{
    public class CustomQueryTranslationPreprocessorFactory : IQueryTranslationPreprocessorFactory
    {
        private readonly IQueryTranslationPlugin[] translationPlugins;

        public CustomQueryTranslationPreprocessorFactory(QueryTranslationPreprocessorDependencies dependencies,
            RelationalQueryTranslationPreprocessorDependencies relationalDependencies, IQueryTranslationPlugin[] translationPlugins)
        {
            Dependencies = dependencies;
            RelationalDependencies = relationalDependencies;
            this.translationPlugins = translationPlugins;
        }

        protected QueryTranslationPreprocessorDependencies Dependencies { get; }

        protected RelationalQueryTranslationPreprocessorDependencies RelationalDependencies;

        public QueryTranslationPreprocessor Create(QueryCompilationContext queryCompilationContext)
            => new CustomQueryTranslationPreprocessor(Dependencies, RelationalDependencies, queryCompilationContext, translationPlugins);
    }
}