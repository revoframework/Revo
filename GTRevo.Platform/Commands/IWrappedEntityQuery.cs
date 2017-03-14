namespace GTRevo.Platform.Commands
{
    public interface IWrappedEntityQuery<TEntity, TWrapper> : IQuery<TWrapper>
        where TWrapper : IEntityQueryableWrapper<TEntity>
    {
    }
}
