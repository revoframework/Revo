using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.EFCore.DataAccess.Query
{
    public class CustomQueryTranslationPreprocessorFactory : IQueryTranslationPreprocessorFactory
    {
        private readonly IServiceProvider serviceProvider;

        public CustomQueryTranslationPreprocessorFactory(QueryTranslationPreprocessorDependencies dependencies,
            RelationalQueryTranslationPreprocessorDependencies relationalDependencies, IServiceProvider serviceProvider)
        {
            Dependencies = dependencies;
            RelationalDependencies = relationalDependencies;
            this.serviceProvider = serviceProvider;
        }

        protected QueryTranslationPreprocessorDependencies Dependencies { get; }

        protected RelationalQueryTranslationPreprocessorDependencies RelationalDependencies;

        public QueryTranslationPreprocessor Create(QueryCompilationContext queryCompilationContext)
        {
            var translationPlugins = serviceProvider.GetServices<IQueryTranslationPlugin>();
            return new CustomQueryTranslationPreprocessor(Dependencies, RelationalDependencies,
                queryCompilationContext,  translationPlugins.ToArray());
        }
    }
}
