using System.Linq.Expressions;

namespace Revo.EFCore.DataAccess.Query
{
    public interface IQueryTranslationPlugin
    {
        Expression Preprocess(Expression query);
    }
}