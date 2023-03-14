namespace Revo.DataAccess.Entities
{
    public interface ICrudRepositoryFactory<out TRepository> where TRepository : IReadRepository
    {
        TRepository Create();
    }
}
