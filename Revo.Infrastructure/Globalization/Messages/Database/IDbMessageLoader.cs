namespace Revo.Infrastructure.Globalization.Messages.Database
{
    public interface IDbMessageLoader
    {
        void EnsureLoaded();
        void Reload();
    }
}