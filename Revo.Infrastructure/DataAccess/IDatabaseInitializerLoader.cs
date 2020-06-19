namespace Revo.Infrastructure.DataAccess
{
    public interface IDatabaseInitializerLoader
    {
        void EnsureDatabaseInitialized();
    }
}