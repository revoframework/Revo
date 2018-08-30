namespace Revo.EF6.DataAccess.Entities
{
    public interface IDbMemberEntry<TEntity, TProperty>
        where TEntity : class
    {
        string Name { get; }
        TProperty CurrentValue { get; set; }
    }
}
