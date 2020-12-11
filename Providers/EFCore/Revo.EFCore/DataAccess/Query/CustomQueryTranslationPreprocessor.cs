using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Revo.EFCore.DataAccess.Query
{
    public class CustomQueryTranslationPreprocessor : RelationalQueryTranslationPreprocessor
    {
        private readonly IQueryTranslationPlugin[] translationPlugins;

        public CustomQueryTranslationPreprocessor(QueryTranslationPreprocessorDependencies dependencies,
            RelationalQueryTranslationPreprocessorDependencies relationalDependencies,
            QueryCompilationContext queryCompilationContext,
            IQueryTranslationPlugin[] translationPlugins)
            : base(dependencies, relationalDependencies, queryCompilationContext)
        {
            this.translationPlugins = translationPlugins;
        }

        public override Expression Process(Expression query) => base.Process(Preprocess(query));

        private Expression Preprocess(Expression query)
        {
            var intermed = query;
            foreach (var translationPlugin in translationPlugins)
            {
                intermed = translationPlugin.Preprocess(intermed);
            }

            return intermed;
        }
    }
}