using System.Linq;

namespace GTRevo.Platform.Commands
{
    public class WrappedEntityQueryWithContext<TEntity, TWrapper> : QueryWithContext<IQueryable<TEntity>>,
        IWrappedEntityQuery<TEntity, TWrapper>
        where TWrapper : IEntityQueryableWrapper<TEntity>
    {
    }
}
