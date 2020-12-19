using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using Revo.Core.Core;

namespace Revo.EFCore.DataAccess.Query
{
    public class CustomQueryTranslationPreprocessorFactory : IQueryTranslationPreprocessorFactory
    {
        private readonly IServiceLocator serviceLocator;

        public CustomQueryTranslationPreprocessorFactory(QueryTranslationPreprocessorDependencies dependencies,
            RelationalQueryTranslationPreprocessorDependencies relationalDependencies, IServiceLocator serviceLocator)
        {
            Dependencies = dependencies;
            RelationalDependencies = relationalDependencies;
            this.serviceLocator = serviceLocator;
        }

        protected QueryTranslationPreprocessorDependencies Dependencies { get; }

        protected RelationalQueryTranslationPreprocessorDependencies RelationalDependencies;

        public QueryTranslationPreprocessor Create(QueryCompilationContext queryCompilationContext)
        {
            var translationPlugins = serviceLocator.GetAll<IQueryTranslationPlugin>();
            return new CustomQueryTranslationPreprocessor(Dependencies, RelationalDependencies,
                queryCompilationContext,  translationPlugins.ToArray());
        }
    }
}